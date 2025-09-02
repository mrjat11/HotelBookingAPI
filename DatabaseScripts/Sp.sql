USE [HotelDB]
GO
/****** Object:  StoredProcedure [dbo].[spAddAmenity] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Inserts a new amenity into the Amenities table.
-- Prevents duplicates based on the amenity name.
CREATE   PROCEDURE [dbo].[spAddAmenity]
@Name NVARCHAR(100),
@Description NVARCHAR(255),
@CreatedBy NVARCHAR(100),
@AmenityID INT OUTPUT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Check if an amenity with the same name already exists to avoid duplication.
IF EXISTS (SELECT 1 FROM Amenities WHERE Name = @Name)
BEGIN
SET @Status = 0;
SET @Message = 'Amenity already exists.';
END
ELSE
BEGIN
-- Insert the new amenity record.
INSERT INTO Amenities (Name, Description, CreatedBy, CreatedDate, IsActive)
VALUES (@Name, @Description, @CreatedBy, GETDATE(), 1);
-- Retrieve the ID of the newly inserted amenity.
SET @AmenityID = SCOPE_IDENTITY();
SET @Status = 1;
SET @Message = 'Amenity added successfully.';
END
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
SET @Status = 0;
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spAddGuestsToReservation]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spAddGuestsToReservation]
    @UserID INT,
    @ReservationID INT,
    @GuestDetails GuestDetailsTableType READONLY,
    @Status BIT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.

    BEGIN TRY
        BEGIN TRANSACTION
            -- Validate the existence of the user
            IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID AND IsActive = 1)
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'User does not exist or inactive.';
                RETURN;
            END

            -- Validate that all RoomIDs are part of the reservation
            IF EXISTS (
                SELECT 1 FROM @GuestDetails gd
                WHERE NOT EXISTS (
                    SELECT 1 FROM ReservationRooms rr
                    WHERE rr.ReservationID = @ReservationID AND rr.RoomID = gd.RoomID
                )
            )
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'One or more RoomIDs are not valid for this reservation.';
                RETURN;
            END

            -- Create a temporary table to store Guest IDs with ReservationRoomID
            CREATE TABLE #TempGuests
            (
                TempID INT IDENTITY(1,1),
                GuestID INT,
                ReservationRoomID INT
            );

            -- Insert guests into Guests table and retrieve IDs
            INSERT INTO Guests (UserID, FirstName, LastName, Email, Phone, AgeGroup, Address, CountryID, StateID, CreatedBy, CreatedDate)
            SELECT @UserID, gd.FirstName, gd.LastName, gd.Email, gd.Phone, gd.AgeGroup, gd.Address, gd.CountryId, gd.StateId, @UserID, GETDATE()
            FROM @GuestDetails gd;

            -- Capture the Guest IDs and the corresponding ReservationRoomID
            INSERT INTO #TempGuests (GuestID, ReservationRoomID)
            SELECT SCOPE_IDENTITY(), rr.ReservationRoomID
            FROM @GuestDetails gd
            JOIN ReservationRooms rr ON gd.RoomID = rr.RoomID AND rr.ReservationID = @ReservationID;

            -- Link each new guest to a room in the reservation
            INSERT INTO ReservationGuests (ReservationRoomID, GuestID)
            SELECT ReservationRoomID, GuestID
            FROM #TempGuests;

            SET @Status = 1; -- Success
            SET @Message = 'All guests added successfully.';
            COMMIT TRANSACTION;

            -- Cleanup the temporary table
            DROP TABLE #TempGuests;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();

        -- Cleanup the temporary table in case of failure
        IF OBJECT_ID('tempdb..#TempGuests') IS NOT NULL
            DROP TABLE #TempGuests;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spAddRoomAmenity]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Insert Procedure for RoomAmenities
CREATE   PROCEDURE [dbo].[spAddRoomAmenity]
@RoomTypeID INT,
@AmenityID INT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID) OR
NOT EXISTS (SELECT 1 FROM Amenities WHERE AmenityID = @AmenityID)
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'Room type or amenity does not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
IF EXISTS (SELECT 1 FROM RoomAmenities WHERE RoomTypeID = @RoomTypeID AND AmenityID = @AmenityID)
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'This room amenity link already exists.';
ROLLBACK TRANSACTION;
RETURN;
END
INSERT INTO RoomAmenities (RoomTypeID, AmenityID)
VALUES (@RoomTypeID, @AmenityID);
SET @Status = 1; -- Success
SET @Message = 'Room amenity added successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spAddUser]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Add a New User
CREATE PROCEDURE [dbo].[spAddUser]
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @CreatedBy NVARCHAR(100),
    @UserID INT OUTPUT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check if email or password is null
        IF @Email IS NULL OR @PasswordHash IS NULL
        BEGIN
            SET @ErrorMessage = 'Email and Password cannot be null.';
            SET @UserID = -1;
            RETURN;
        END

        -- Check if email already exists in the system
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            SET @ErrorMessage = 'A user with the given email already exists.';
            SET @UserID = -1;
            RETURN;
        END

        -- Default role ID for new users
        DECLARE @DefaultRoleID INT = 2; -- Assuming 'Guest' role ID is 2

        BEGIN TRANSACTION
            INSERT INTO Users (RoleID, Email, PasswordHash, CreatedBy, CreatedDate)
            VALUES (@DefaultRoleID, @Email, @PasswordHash, @CreatedBy, GETDATE());

            SET @UserID = SCOPE_IDENTITY(); -- Retrieve the newly created UserID
            SET @ErrorMessage = NULL;
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        -- Handle exceptions
        ROLLBACK TRANSACTION
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @UserID = -1;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spAssignUserRole]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Assign a Role to User
CREATE PROCEDURE [dbo].[spAssignUserRole]
    @UserID INT,
    @RoleID INT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check if the user exists
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
        BEGIN
            SET @ErrorMessage = 'User not found.';
            RETURN;
        END

        -- Check if the role exists
        IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE RoleID = @RoleID)
        BEGIN
            SET @ErrorMessage = 'Role not found.';
            RETURN;
        END

        -- Update user role
        BEGIN TRANSACTION
            UPDATE Users SET RoleID = @RoleID WHERE UserID = @UserID;
        COMMIT TRANSACTION

        SET @ErrorMessage = NULL;
    END TRY
    BEGIN CATCH
        -- Handle exceptions
        ROLLBACK TRANSACTION
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spBulkInsertAmenities]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Performs a bulk insert of amenities into the Amenities table.
-- Ensures that no duplicate names are inserted.
CREATE   PROCEDURE [dbo].[spBulkInsertAmenities]
@Amenities AmenityInsertType READONLY,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Check for duplicate names within the insert dataset.
IF EXISTS (
SELECT 1
FROM @Amenities
GROUP BY Name
HAVING COUNT(*) > 1
)
BEGIN
SET @Status = 0;
SET @Message = 'Duplicate names found within the new data.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Check for existing names in the Amenities table that might conflict with the new data.
IF EXISTS (
SELECT 1
FROM @Amenities a
WHERE EXISTS (
SELECT 1 FROM Amenities WHERE Name = a.Name
)
)
BEGIN
SET @Status = 0;
SET @Message = 'One or more names conflict with existing records.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Insert new amenities ensuring there are no duplicates by name.
INSERT INTO Amenities (Name, Description, CreatedBy, CreatedDate, IsActive)
SELECT Name, Description, CreatedBy, GETDATE(), 1
FROM @Amenities;
-- Check if any records were actually inserted.
IF @@ROWCOUNT = 0
BEGIN
SET @Status = 0;
SET @Message = 'No records inserted. Please check the input data.';
ROLLBACK TRANSACTION;
END
ELSE
BEGIN
SET @Status = 1;
SET @Message = 'Bulk insert completed successfully.';
COMMIT TRANSACTION;
END
END TRY
BEGIN CATCH
-- Handle any errors that occur during the transaction.
ROLLBACK TRANSACTION;
SET @Status = 0;
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spBulkInsertRoomAmenities]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Bulk Insert into RoomAmenities for a Single RoomTypeID
CREATE   PROCEDURE [dbo].[spBulkInsertRoomAmenities]
@RoomTypeID INT,
@AmenityIDs AmenityIDTableType READONLY,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
-- Check if the RoomTypeID exists
IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'Room type does not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Check if all AmenityIDs exist
IF EXISTS (SELECT 1 FROM @AmenityIDs WHERE AmenityID NOT IN (SELECT AmenityID FROM Amenities))
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'One or more amenities do not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Insert AmenityIDs that do not already exist for the given RoomTypeID
INSERT INTO RoomAmenities (RoomTypeID, AmenityID)
SELECT @RoomTypeID, a.AmenityID 
FROM @AmenityIDs a
WHERE NOT EXISTS (
SELECT 1 
FROM RoomAmenities ra
WHERE ra.RoomTypeID = @RoomTypeID AND ra.AmenityID = a.AmenityID
);
SET @Status = 1; -- Success
SET @Message = 'Room amenities added successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spBulkUpdateAmenities]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Updates multiple amenities in the Amenities table using a provided list.
-- Applies updates to the Name, Description, and IsActive status.
CREATE   PROCEDURE [dbo].[spBulkUpdateAmenities]
@AmenityUpdates AmenityUpdateType READONLY,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Check for duplicate names within the update dataset.
IF EXISTS (
SELECT 1
FROM @AmenityUpdates u
GROUP BY u.Name
HAVING COUNT(*) > 1
)
BEGIN
SET @Status = 0;
SET @Message = 'Duplicate names found within the update data.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Check for duplicate names in existing data.
IF EXISTS (
SELECT 1
FROM @AmenityUpdates u
JOIN Amenities a ON u.Name = a.Name AND u.AmenityID != a.AmenityID
)
BEGIN
SET @Status = 0;
SET @Message = 'One or more names conflict with existing records.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Update amenities based on the provided data.
UPDATE a
SET a.Name = u.Name,
a.Description = u.Description,
a.IsActive = u.IsActive
FROM Amenities a
INNER JOIN @AmenityUpdates u ON a.AmenityID = u.AmenityID;
-- Check if any records were actually updated.
IF @@ROWCOUNT = 0
BEGIN
SET @Status = 0;
SET @Message = 'No records updated. Please check the input data.';
END
ELSE
BEGIN
SET @Status = 1;
SET @Message = 'Bulk update completed successfully.';
END
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
-- Roll back the transaction and handle the error.
ROLLBACK TRANSACTION;
SET @Status = 0;
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spBulkUpdateAmenityStatus]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Updates the active status of multiple amenities in the Amenities table.
-- Takes a list of amenity IDs and their new IsActive status.
CREATE   PROCEDURE [dbo].[spBulkUpdateAmenityStatus]
@AmenityStatuses AmenityStatusType READONLY,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Update the IsActive status for amenities based on the provided AmenityID.
UPDATE a
SET a.IsActive = s.IsActive
FROM Amenities a
INNER JOIN @AmenityStatuses s ON a.AmenityID = s.AmenityID;
-- Check if any records were actually updated.
SET @Status = 1; -- Success
SET @Message = 'Bulk status update completed successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
-- Roll back the transaction if an error occurs.
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spBulkUpdateRoomAmenities]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Bulk Update in RoomAmenities of a single @RoomTypeID
CREATE   PROCEDURE [dbo].[spBulkUpdateRoomAmenities]
@RoomTypeID INT,
@AmenityIDs AmenityIDTableType READONLY,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'Room type does not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
DECLARE @Count INT;
SELECT @Count = COUNT(*) FROM Amenities WHERE AmenityID IN (SELECT AmenityID FROM @AmenityIDs);
IF @Count <> (SELECT COUNT(*) FROM @AmenityIDs)
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'One or more amenities do not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
DELETE FROM RoomAmenities WHERE RoomTypeID = @RoomTypeID;
INSERT INTO RoomAmenities (RoomTypeID, AmenityID)
SELECT @RoomTypeID, AmenityID FROM @AmenityIDs;
SET @Status = 1; -- Success
SET @Message = 'Room amenities updated successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spCalculateCancellationCharges]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Calculate Cancellation Charges
-- Calculates the cancellation charges based on the policies.
CREATE   PROCEDURE [dbo].[spCalculateCancellationCharges]
    @ReservationID INT,
    @RoomsCancelled RoomIDTableType READONLY,
    @TotalCost DECIMAL(10,2) OUTPUT,
    @CancellationCharge DECIMAL(10,2) OUTPUT,
    @CancellationPercentage DECIMAL(10,2) OUTPUT,
    @PolicyDescription NVARCHAR(255) OUTPUT,
    @Status BIT OUTPUT,
    @Message NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CheckInDate DATE;
    DECLARE @TotalRoomsCount INT, @CancelledRoomsCount INT;

    BEGIN TRY
        -- Fetch check-in date
        SELECT @CheckInDate = CheckInDate FROM Reservations WHERE ReservationID = @ReservationID;
        IF @CheckInDate IS NULL
        BEGIN
            SET @Status = 0; -- Failure
            SET @Message = 'No reservation found with the given ID.';
            RETURN;
        END

        -- Determine if the cancellation is full or partial
        SELECT @TotalRoomsCount = COUNT(*) FROM ReservationRooms WHERE ReservationID = @ReservationID;
        SELECT @CancelledRoomsCount = COUNT(*) FROM @RoomsCancelled;

        IF @CancelledRoomsCount = @TotalRoomsCount
        BEGIN
            -- Full cancellation: Calculate based on total reservation cost
            SELECT @TotalCost = SUM(TotalAmount)
            FROM Payments 
            WHERE ReservationID = @ReservationID;
        END
        ELSE
        BEGIN
            -- Partial cancellation: Calculate based on specific rooms' costs from PaymentDetails
            SELECT @TotalCost = SUM(pd.Amount)
            FROM PaymentDetails pd
            INNER JOIN ReservationRooms rr ON pd.ReservationRoomID = rr.ReservationRoomID
            INNER JOIN @RoomsCancelled rc ON rr.RoomID = rc.RoomID
            WHERE rr.ReservationID = @ReservationID;
        END

        -- Check if total cost was calculated
        IF @TotalCost IS NULL
        BEGIN
            SET @Status = 0; -- Failure
            SET @Message = 'Failed to calculate total costs.';
            RETURN;
        END

        -- Fetch the appropriate cancellation policy based on the check-in date
        SELECT TOP 1 @CancellationPercentage = CancellationChargePercentage, 
                     @PolicyDescription = Description
        FROM CancellationPolicies
        WHERE EffectiveFromDate <= @CheckInDate AND EffectiveToDate >= @CheckInDate
        ORDER BY EffectiveFromDate DESC; -- In case of overlapping policies, the most recent one is used

        -- Calculate the cancellation charge
        SET @CancellationCharge = @TotalCost * (@CancellationPercentage / 100);

        SET @Status = 1; -- Success
        SET @Message = 'Calculation successful';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spCalculateRoomCosts]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- This stored procedure will calculate and return the Total Cost and Room wise Cost Breakup
CREATE   PROCEDURE [dbo].[spCalculateRoomCosts]
    @RoomIDs RoomIDTableType READONLY,
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @Amount DECIMAL(10, 2) OUTPUT,        -- Base total cost before tax
    @GST DECIMAL(10, 2) OUTPUT,           -- GST amount based on 18%
    @TotalAmount DECIMAL(10, 2) OUTPUT    -- Total cost including GST
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate the number of nights based on CheckInDate and CheckOutDate
    DECLARE @NumberOfNights INT = DATEDIFF(DAY, @CheckInDate, @CheckOutDate);
    
    IF @NumberOfNights <= 0
    BEGIN
        SET @Amount = 0;
        SET @GST = 0;
        SET @TotalAmount = 0;
        RETURN; -- Exit if the number of nights is zero or negative, which shouldn't happen
    END

    -- Select Individual Rooms Price details
    SELECT 
        r.RoomID,
        r.RoomNumber,
        r.Price AS RoomPrice,
        @NumberOfNights AS NumberOfNights,
        r.Price * @NumberOfNights AS TotalPrice
    FROM 
        Rooms r
    INNER JOIN 
        @RoomIDs ri ON r.RoomID = ri.RoomID;

    -- Calculate total cost (base amount) from the rooms identified by RoomIDs multiplied by NumberOfNights
    SELECT @Amount = SUM(Price * @NumberOfNights) FROM Rooms
    WHERE RoomID IN (SELECT RoomID FROM @RoomIDs);

    -- Calculate GST as 18% of the Amount
    SET @GST = @Amount * 0.18;

    -- Calculate Total Amount as Amount plus GST
    SET @TotalAmount = @Amount + @GST;
END;
GO
/****** Object:  StoredProcedure [dbo].[spCreateCancellationRequest]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create Cancellation Request
-- This Stored Procedure creates a cancellation request after validating the provided information.
CREATE   PROCEDURE [dbo].[spCreateCancellationRequest]
    @UserID INT,
    @ReservationID INT,
    @RoomsCancelled RoomIDTableType READONLY, -- Table-valued parameter
    @CancellationReason NVARCHAR(MAX),
    @Status BIT OUTPUT, -- Output parameter for operation status
    @Message NVARCHAR(255) OUTPUT, -- Output parameter for operation message
    @CancellationRequestID INT OUTPUT -- Output parameter to store the newly created CancellationRequestID
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.
    DECLARE @CancellationType NVARCHAR(50);
    DECLARE @TotalRooms INT, @CancelledRoomsCount INT, @RemainingRoomsCount INT;
    DECLARE @ExistingStatus NVARCHAR(50);
    DECLARE @CheckInDate DATE, @CheckOutDate DATE;

    -- Retrieve reservation details
    SELECT @ExistingStatus = Status, @CheckInDate = CheckInDate, @CheckOutDate = CheckOutDate
    FROM Reservations
    WHERE ReservationID = @ReservationID;

    -- Validation for reservation status and dates
    IF @ExistingStatus = 'Cancelled' OR GETDATE() >= @CheckInDate
    BEGIN
        SET @Status = 0; -- Failure
        SET @Message = 'Cancellation not allowed. Reservation already fully cancelled or past check-in date.';
        RETURN;
    END

    -- Prevent cancellation of already cancelled or pending cancellation rooms
    IF EXISTS (
        SELECT 1 
        FROM CancellationDetails cd
        JOIN CancellationRequests cr ON cd.CancellationRequestID = cr.CancellationRequestID
        JOIN ReservationRooms rr ON cd.ReservationRoomID = rr.ReservationRoomID
        JOIN @RoomsCancelled rc ON rr.RoomID = rc.RoomID
        WHERE cr.ReservationID = @ReservationID AND cr.Status IN ('Approved', 'Pending')
    )
    BEGIN
        SET @Status = 0; -- Failure
        SET @Message = 'One or more rooms have already been cancelled or cancellation is pending.';
        RETURN;
    END

    SELECT @TotalRooms = COUNT(*) FROM ReservationRooms WHERE ReservationID = @ReservationID;
    SELECT @CancelledRoomsCount = COUNT(*) FROM CancellationDetails cd
           JOIN CancellationRequests cr ON cd.CancellationRequestID = cr.CancellationRequestID
           WHERE cr.ReservationID = @ReservationID AND cr.Status IN ('Approved');

    -- Calculate remaining rooms that are not yet cancelled
    SET @RemainingRoomsCount = @TotalRooms - @CancelledRoomsCount;

    -- Determine the type of cancellation based on remaining rooms to be cancelled
    IF (@RemainingRoomsCount = (SELECT COUNT(*) FROM @RoomsCancelled))
        SET @CancellationType = 'Full'
    ELSE
        SET @CancellationType = 'Partial';

    BEGIN TRY
        BEGIN TRANSACTION
            -- Insert into CancellationRequests
            INSERT INTO CancellationRequests (ReservationID, UserID, CancellationType, RequestedOn, Status, CancellationReason)
            VALUES (@ReservationID, @UserID, @CancellationType, GETDATE(), 'Pending', @CancellationReason);

            SET @CancellationRequestID = SCOPE_IDENTITY();

            -- Insert into CancellationDetails for rooms not yet cancelled
            INSERT INTO CancellationDetails (CancellationRequestID, ReservationRoomID)
            SELECT @CancellationRequestID, rr.ReservationRoomID 
            FROM ReservationRooms rr 
            JOIN @RoomsCancelled rc ON rr.RoomID = rc.RoomID
            WHERE rr.ReservationID = @ReservationID;

        COMMIT TRANSACTION;
        SET @Status = 1; -- Success
        SET @Message = 'Cancellation request created successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spCreateReservation]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Creating a New Reservation
-- This stored procedure will ensure that both the user exists and the selected rooms are available before creating a reservation
CREATE   PROCEDURE [dbo].[spCreateReservation]
    @UserID INT,
    @RoomIDs RoomIDTableType READONLY, -- Using the table-valued parameter
    @CheckInDate DATE,
    @CheckOutDate DATE,
    @CreatedBy NVARCHAR(100),
    @Message NVARCHAR(255) OUTPUT,
    @Status BIT OUTPUT,
    @ReservationID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.

    BEGIN TRY
        BEGIN TRANSACTION

            -- Check if the user exists
            IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID AND IsActive = 1)
            BEGIN
                SET @Message = 'User does not exist or inactive.';
                SET @Status = 0; -- 0 means Failed
                RETURN;
            END

            -- Check if all rooms are available
            IF EXISTS (SELECT 1 FROM Rooms WHERE RoomID IN (SELECT RoomID FROM @RoomIDs) AND Status <> 'Available')
            BEGIN
                SET @Message = 'One or more rooms are not available.';
                SET @Status = 0; -- 0 means Failed
                RETURN;
            END

            -- Calculate the number of nights between CheckInDate and CheckOutDate
            DECLARE @NumberOfNights INT = DATEDIFF(DAY, @CheckInDate, @CheckOutDate);
            IF @NumberOfNights <= 0
            BEGIN
                SET @Message = 'Check-out date must be later than check-in date.';
                SET @Status = 0; -- 0 means Failed
                RETURN;
            END

            -- Calculate the base cost of the rooms for the number of nights and add GST
            DECLARE @BaseCost DECIMAL(10, 2);
            SELECT @BaseCost = SUM(Price * @NumberOfNights) FROM Rooms
            WHERE RoomID IN (SELECT RoomID FROM @RoomIDs);

            -- Calculate Total Amount including 18% GST
            DECLARE @TotalAmount DECIMAL(10, 2) = @BaseCost * 1.18;

            -- Create the Reservation
            INSERT INTO Reservations (UserID, BookingDate, CheckInDate, CheckOutDate, NumberOfNights, TotalCost, Status, CreatedBy, CreatedDate)
            VALUES (@UserID, GETDATE(), @CheckInDate, @CheckOutDate, @NumberOfNights, @TotalAmount, 'Reserved', @CreatedBy, GETDATE());

            SET @ReservationID = SCOPE_IDENTITY();

            -- Assign rooms to the reservation and update room status
            INSERT INTO ReservationRooms (ReservationID, RoomID, CheckInDate, CheckOutDate)
            SELECT @ReservationID, RoomID, @CheckInDate, @CheckOutDate FROM @RoomIDs;

            -- Update the status of the rooms to 'Occupied'
            UPDATE Rooms
            SET Status = 'Occupied'
            WHERE RoomID IN (SELECT RoomID FROM @RoomIDs);

            SET @Message = 'Reservation created successfully.';
            SET @Status = 1; -- 1 means Success
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @Message = ERROR_MESSAGE();
        SET @Status = 0; -- 0 means Failed
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spCreateRoom]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create Room
CREATE   PROCEDURE [dbo].[spCreateRoom]
    @RoomNumber NVARCHAR(10),
    @RoomTypeID INT,
    @Price DECIMAL(10,2),
    @BedType NVARCHAR(50),
    @ViewType NVARCHAR(50),
    @Status NVARCHAR(50),
    @IsActive BIT,
    @CreatedBy NVARCHAR(100),
    @NewRoomID INT OUTPUT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            -- Check if the provided RoomTypeID exists in the RoomTypes table
            IF EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
            BEGIN
                -- Ensure the room number is unique
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = @RoomNumber)
                BEGIN
                    INSERT INTO Rooms (RoomNumber, RoomTypeID, Price, BedType, ViewType, Status, IsActive, CreatedBy, CreatedDate)
                    VALUES (@RoomNumber, @RoomTypeID, @Price, @BedType, @ViewType, @Status, @IsActive, @CreatedBy, GETDATE())

                    SET @NewRoomID = SCOPE_IDENTITY()
                    SET @StatusCode = 0 -- Success
                    SET @Message = 'Room created successfully.'
                END
                ELSE
                BEGIN
                    SET @StatusCode = 1 -- Failure due to duplicate room number
                    SET @Message = 'Room number already exists.'
                END
            END
            ELSE
            BEGIN
                SET @StatusCode = 3 -- Failure due to invalid RoomTypeID
                SET @Message = 'Invalid Room Type ID provided.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER()
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spCreateRoomType]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create Room Type
CREATE PROCEDURE [dbo].[spCreateRoomType]
    @TypeName NVARCHAR(50),
    @AccessibilityFeatures NVARCHAR(255),
    @Description NVARCHAR(255),
    @CreatedBy NVARCHAR(100),
    @NewRoomTypeID INT OUTPUT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE TypeName = @TypeName)
            BEGIN
                INSERT INTO RoomTypes (TypeName, AccessibilityFeatures, Description, CreatedBy, CreatedDate)
                VALUES (@TypeName, @AccessibilityFeatures, @Description, @CreatedBy, GETDATE())

                SET @NewRoomTypeID = SCOPE_IDENTITY()
                SET @StatusCode = 0 -- Success
                SET @Message = 'Room type created successfully.'
            END
            ELSE
            BEGIN
                SET @StatusCode = 1 -- Failure due to duplicate name
                SET @Message = 'Room type name already exists.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER() -- SQL Server error number
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spDeleteAllRoomAmenitiesByAmenityID]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Deleting All RoomAmenities of a Single AmenityID
CREATE   PROCEDURE [dbo].[spDeleteAllRoomAmenitiesByAmenityID]
@AmenityID INT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
-- Delete all amenities for the specified Amenity ID
DELETE FROM RoomAmenities WHERE AmenityID = @AmenityID;
SET @Status = 1; -- Success
SET @Message = 'All amenities for the Amenity ID have been deleted successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spDeleteAllRoomAmenitiesByRoomTypeID]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Deleting All RoomAmenities of a Single RoomTypeID
CREATE   PROCEDURE [dbo].[spDeleteAllRoomAmenitiesByRoomTypeID]
@RoomTypeID INT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
-- Delete all amenities for the specified room type
DELETE FROM RoomAmenities WHERE RoomTypeID = @RoomTypeID;
SET @Status = 1; -- Success
SET @Message = 'All amenities for the room type have been deleted successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spDeleteAmenity]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Soft deletes an amenity by setting its IsActive flag to 0.
-- Checks if the amenity exists before marking it as inactive.
CREATE   PROCEDURE [dbo].[spDeleteAmenity]
@AmenityID INT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Check if the amenity exists before attempting to delete.
IF NOT EXISTS (SELECT 1 FROM Amenities WHERE AmenityID = @AmenityID)
BEGIN
SET @Status = 0;
SET @Message = 'Amenity does not exist.';
END
ELSE
BEGIN
-- Update the IsActive flag to 0 to soft delete the amenity.
UPDATE Amenities
SET IsActive = 0
WHERE AmenityID = @AmenityID;
SET @Status = 1;
SET @Message = 'Amenity deleted successfully.';
END
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
-- Roll back the transaction if an error occurs.
ROLLBACK TRANSACTION;
SET @Status = 0;
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spDeleteRoom]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Delete Room (Soft Delete)
CREATE   PROCEDURE [dbo].[spDeleteRoom]
    @RoomID INT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            -- Ensure no active reservations exist for the room
            IF NOT EXISTS (SELECT 1 FROM Reservations WHERE RoomID = @RoomID AND Status NOT IN ('Checked-out', 'Cancelled'))
            BEGIN
                -- Verify the room exists and is currently active before deactivating
                IF EXISTS (SELECT 1 FROM Rooms WHERE RoomID = @RoomID AND IsActive = 1)
                BEGIN
                    -- Instead of deleting, we update the IsActive flag to false
                    UPDATE Rooms
                    SET IsActive = 0  -- Set IsActive to false to indicate the room is no longer active
                    WHERE RoomID = @RoomID

                    SET @StatusCode = 0 -- Success
                    SET @Message = 'Room deactivated successfully.'
                END
                ELSE
                BEGIN
                    SET @StatusCode = 2 -- Failure due to room not found or already deactivated
                    SET @Message = 'Room not found or already deactivated.'
                END
            END
            ELSE
            BEGIN
                SET @StatusCode = 1 -- Failure due to active reservations
                SET @Message = 'Room cannot be deactivated, there are active reservations.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER()
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spDeleteRoomType]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Delete Room Type By Id
CREATE PROCEDURE [dbo].[spDeleteRoomType]
    @RoomTypeID INT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
   
            -- Check for existing rooms linked to this room type
            IF NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomTypeID = @RoomTypeID)
            BEGIN
                IF EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
                BEGIN
                    DELETE FROM RoomTypes WHERE RoomTypeID = @RoomTypeID
                    SET @StatusCode = 0 -- Success
                    SET @Message = 'Room type deleted successfully.'
                END
                ELSE
                BEGIN
                    SET @StatusCode = 2 -- Failure due to not found
                    SET @Message = 'Room type not found.'
                END
            END
            ELSE
            BEGIN
                SET @StatusCode = 1 -- Failure due to dependency
                SET @Message = 'Cannot delete room type as it is being referenced by one or more rooms.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER() -- SQL Server error number
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spDeleteSingleRoomAmenity]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Deleting a Single RoomAmenities based on RoomTypeID and AmenityID
CREATE   PROCEDURE [dbo].[spDeleteSingleRoomAmenity]
@RoomTypeID INT,
@AmenityID INT,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @Exists BIT;
SELECT @Exists = COUNT(*) FROM RoomAmenities WHERE RoomTypeID = @RoomTypeID AND AmenityID = @AmenityID;
IF @Exists = 0
BEGIN
SET @Status = 0; -- Failure
SET @Message = 'The specified RoomTypeID and AmenityID combination does not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Delete the specified room amenity
DELETE FROM RoomAmenities
WHERE RoomTypeID = @RoomTypeID AND AmenityID = @AmenityID;
SET @Status = 1; -- Success
SET @Message = 'Room amenity deleted successfully.';
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
ROLLBACK TRANSACTION;
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spFetchAmenities]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Fetches amenities based on their active status.
-- If @IsActive is provided, it returns amenities filtered by the active status.
CREATE   PROCEDURE [dbo].[spFetchAmenities]
@IsActive BIT = NULL,
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
-- Retrieve all amenities or filter by active status based on the input parameter.
IF @IsActive IS NULL
SELECT * FROM Amenities;
ELSE
SELECT * FROM Amenities WHERE IsActive = @IsActive;
-- Return success status and message.
SET @Status = 1; -- Success
SET @Message = 'Data retrieved successfully.';
END TRY
BEGIN CATCH
-- Handle errors and return failure status.
SET @Status = 0; -- Failure
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spFetchAmenityByID]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Fetches a specific amenity based on its ID.
-- Returns the details of the amenity if it exists.
CREATE   PROCEDURE [dbo].[spFetchAmenityByID]
@AmenityID INT
AS
BEGIN
SET NOCOUNT ON;
SELECT AmenityID, Name, Description, IsActive FROM Amenities 
WHERE AmenityID = @AmenityID;
END;
GO
/****** Object:  StoredProcedure [dbo].[spFetchRoomAmenitiesByRoomTypeID] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Fetching All RoomAmenities by RoomTypeID
CREATE   PROCEDURE [dbo].[spFetchRoomAmenitiesByRoomTypeID]
@RoomTypeID INT
AS
BEGIN
SET NOCOUNT ON;
SELECT a.AmenityID, a.Name, a.Description, a.IsActive
FROM RoomAmenities ra
JOIN Amenities a ON ra.AmenityID = a.AmenityID
WHERE ra.RoomTypeID = @RoomTypeID;
END;
GO
/****** Object:  StoredProcedure [dbo].[spFetchRoomTypesByAmenityID]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Fetching All RoomTypes by AmenityID
CREATE   PROCEDURE [dbo].[spFetchRoomTypesByAmenityID]
@AmenityID INT
AS
BEGIN
SET NOCOUNT ON;
SELECT rt.RoomTypeID, rt.TypeName, rt.Description, rt.AccessibilityFeatures, rt.IsActive
FROM RoomAmenities ra
JOIN RoomTypes rt ON ra.RoomTypeID = rt.RoomTypeID
WHERE ra.AmenityID = @AmenityID;
END;
GO
/****** Object:  StoredProcedure [dbo].[spGetAllCancellations]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Get All Cancellations
-- This procedure fetches all cancellations based on the optional status filter.
CREATE   PROCEDURE [dbo].[spGetAllCancellations]
    @Status NVARCHAR(50) = NULL,
    @DateFrom DATETIME = NULL,
    @DateTo DATETIME = NULL,
    @StatusOut BIT OUTPUT, -- Output parameter for operation status
    @MessageOut NVARCHAR(255) OUTPUT -- Output parameter for operation message
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX), @Params NVARCHAR(MAX);

    -- Initialize dynamic SQL query
    SET @SQL = N'SELECT CancellationRequestID, ReservationID, UserID, CancellationType, RequestedOn, Status FROM CancellationRequests WHERE 1=1';

    -- Append conditions dynamically based on the input parameters
    IF @Status IS NOT NULL
        SET @SQL += N' AND Status = @Status';
    IF @DateFrom IS NOT NULL
        SET @SQL += N' AND RequestedOn >= @DateFrom';
    IF @DateTo IS NOT NULL
        SET @SQL += N' AND RequestedOn <= @DateTo';

    -- Define parameters for dynamic SQL
    SET @Params = N'@Status NVARCHAR(50), @DateFrom DATETIME, @DateTo DATETIME';

    BEGIN TRY
        -- Execute dynamic SQL
        EXEC sp_executesql @SQL, @Params, @Status = @Status, @DateFrom = @DateFrom, @DateTo = @DateTo;

        -- If successful, set output parameters
        SET @StatusOut = 1; -- Success
        SET @MessageOut = 'Cancellations retrieved successfully.';
    END TRY
    BEGIN CATCH
        -- If an error occurs, set output parameters to indicate failure
        SET @StatusOut = 0; -- Failure
        SET @MessageOut = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spGetAllRoom]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get All Rooms with Optional Filtering
CREATE   PROCEDURE [dbo].[spGetAllRoom]
    @RoomTypeID INT = NULL,     -- Optional filter by Room Type
    @Status NVARCHAR(50) = NULL -- Optional filter by Status
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SQL NVARCHAR(MAX)

    -- Start building the dynamic SQL query
    SET @SQL = 'SELECT RoomID, RoomNumber, RoomTypeID, Price, BedType, ViewType, Status, IsActive FROM Rooms WHERE 1=1'

    -- Append conditions based on the presence of optional parameters
    IF @RoomTypeID IS NOT NULL
        SET @SQL = @SQL + ' AND RoomTypeID = @RoomTypeID'
    
    IF @Status IS NOT NULL
        SET @SQL = @SQL + ' AND Status = @Status'

    -- Execute the dynamic SQL statement
    EXEC sp_executesql @SQL, 
                       N'@RoomTypeID INT, @Status NVARCHAR(50)', 
                       @RoomTypeID, 
                       @Status
END
GO
/****** Object:  StoredProcedure [dbo].[spGetAllRoomTypes]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get All Room Type
CREATE PROCEDURE [dbo].[spGetAllRoomTypes]
 @IsActive BIT = NULL  -- Optional parameter to filter by IsActive status
AS
BEGIN
    SET NOCOUNT ON;
    -- Select users based on active status
    IF @IsActive IS NULL
    BEGIN
        SELECT RoomTypeID, TypeName, AccessibilityFeatures, Description, IsActive FROM RoomTypes
    END
    ELSE
    BEGIN
        SELECT RoomTypeID, TypeName, AccessibilityFeatures, Description, IsActive FROM RoomTypes WHERE IsActive = @IsActive;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[spGetCancellationPolicies]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Get Cancellation Policies
-- This stored procedure retrieves active cancellation policies for display purposes.
CREATE   PROCEDURE [dbo].[spGetCancellationPolicies]
    @Status BIT OUTPUT,    -- Output parameter for status (1 = Success, 0 = Failure)
    @Message NVARCHAR(255) OUTPUT  -- Output parameter for messages
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT PolicyID, Description, CancellationChargePercentage, MinimumCharge, EffectiveFromDate, EffectiveToDate 
        FROM CancellationPolicies
        WHERE EffectiveFromDate <= GETDATE() AND EffectiveToDate >= GETDATE();

        SET @Status = 1;  -- Success
        SET @Message = 'Policies retrieved successfully.';
    END TRY
    BEGIN CATCH
        SET @Status = 0;  -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spGetCancellationsForRefund]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Get Cancellations for Refund
-- This procedure is used by an admin to fetch cancellations that are approved and either have no refund record 
-- or need refund action (Pending or Failed, excluding Completed)
CREATE   PROCEDURE [dbo].[spGetCancellationsForRefund]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        cr.CancellationRequestID, 
        cr.ReservationID,
        cr.UserID,
        cr.CancellationType,
        cr.RequestedOn,
        cr.Status,
        ISNULL(r.RefundID, 0) AS RefundID,  -- Use 0 or another appropriate default value to indicate no refund has been initiated
        ISNULL(r.RefundStatus, 'Not Initiated') AS RefundStatus  -- Use 'Not Initiated' or another appropriate status
    FROM 
        CancellationRequests cr
    LEFT JOIN 
        Refunds r ON cr.CancellationRequestID = r.CancellationRequestID
    WHERE 
        cr.Status = 'Approved' 
        AND (r.RefundStatus IS NULL OR r.RefundStatus IN ('Pending', 'Failed'));
END;
GO
/****** Object:  StoredProcedure [dbo].[spGetRoomAmenitiesByRoomID]    *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Fetch Amenities for a Specific Room
-- Retrieves all amenities associated with a specific room by its RoomID.
-- Inputs: @RoomID - The ID of the room
-- Returns: List of amenities associated with the room type of the specified room
CREATE   PROCEDURE [dbo].[spGetRoomAmenitiesByRoomID]
@RoomID INT
AS
BEGIN
SET NOCOUNT ON; -- Suppresses the 'rows affected' message
SELECT 
a.AmenityID, 
a.Name, 
a.Description
FROM RoomAmenities ra
JOIN Amenities a ON ra.AmenityID = a.AmenityID
JOIN Rooms r ON ra.RoomTypeID = r.RoomTypeID
WHERE r.RoomID = @RoomID
AND a.IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[spGetRoomById]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get Room by Id
CREATE   PROCEDURE [dbo].[spGetRoomById]
    @RoomID INT
AS
BEGIN
    SELECT RoomID, RoomNumber, RoomTypeID, Price, BedType, ViewType, Status, IsActive FROM Rooms WHERE RoomID = @RoomID
END
GO
/****** Object:  StoredProcedure [dbo].[spGetRoomDetailsWithAmenitiesByRoomID]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure to Fetch Room, Room Type, and Amenities Details
-- Retrieves details of a room by its RoomID, including room type and amenities.
-- Inputs: @RoomID - The ID of the room
-- Returns: Details of the room, its room type, and associated amenities
CREATE   PROCEDURE [dbo].[spGetRoomDetailsWithAmenitiesByRoomID]
@RoomID INT
AS
BEGIN
SET NOCOUNT ON; -- Suppresses the 'rows affected' message
-- First, retrieve the basic details of the room along with its room type information
SELECT 
r.RoomID, 
r.RoomNumber, 
r.Price, 
r.BedType, 
r.ViewType, 
r.Status,
rt.RoomTypeID, 
rt.TypeName, 
rt.AccessibilityFeatures, 
rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
WHERE r.RoomID = @RoomID
AND r.IsActive = 1;
-- Next, retrieve the amenities associated with the room type of the specified room
SELECT 
a.AmenityID, 
a.Name, 
a.Description
FROM RoomAmenities ra
JOIN Amenities a ON ra.AmenityID = a.AmenityID
WHERE ra.RoomTypeID IN (SELECT RoomTypeID FROM Rooms WHERE RoomID = @RoomID)
AND a.IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[spGetRoomTypeById]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get Room Type By Id
CREATE PROCEDURE [dbo].[spGetRoomTypeById]
    @RoomTypeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoomTypeID, TypeName, AccessibilityFeatures, Description, IsActive FROM RoomTypes WHERE RoomTypeID = @RoomTypeID
END
GO
/****** Object:  StoredProcedure [dbo].[spGetUserByID]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get User by ID
CREATE PROCEDURE [dbo].[spGetUserByID]
    @UserID INT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if the user exists
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
    BEGIN
        SET @ErrorMessage = 'User not found.';
        RETURN;
    END

    -- Retrieve user details
    SELECT UserID, Email, RoleID, IsActive, LastLogin, CreatedBy, CreatedDate FROM Users WHERE UserID = @UserID;
    SET @ErrorMessage = NULL;
END;
GO
/****** Object:  StoredProcedure [dbo].[spListAllUsers]     ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- List All Users
CREATE PROCEDURE [dbo].[spListAllUsers]
    @IsActive BIT = NULL  -- Optional parameter to filter by IsActive status
AS
BEGIN
    SET NOCOUNT ON;

    -- Select users based on active status
    IF @IsActive IS NULL
    BEGIN
        SELECT UserID, Email, RoleID, IsActive, LastLogin, CreatedBy, CreatedDate FROM Users;
    END
    ELSE
    BEGIN
        SELECT UserID, Email, RoleID, IsActive, LastLogin, CreatedBy, CreatedDate FROM Users 
  WHERE IsActive = @IsActive;
    END
END;
GO
/****** Object:  StoredProcedure [dbo].[spLoginUser]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Login a User
CREATE PROCEDURE [dbo].[spLoginUser]
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @UserID INT OUTPUT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    -- Attempt to retrieve the user based on email and password hash
    SELECT @UserID = UserID FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash;

    -- Check if user ID was set (means credentials are correct)
    IF @UserID IS NOT NULL
    BEGIN
        -- Check if the user is active
        IF EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID AND IsActive = 1)
        BEGIN
            -- Update the last login time
            UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID;
            SET @ErrorMessage = NULL; -- Clear any previous error messages
        END
        ELSE
        BEGIN
            SET @ErrorMessage = 'User account is not active.';
            SET @UserID = NULL; -- Reset the UserID as login should not be considered successful
        END
    END
    ELSE
    BEGIN
        SET @ErrorMessage = 'Invalid Credentials.';
    END
END;
GO
/****** Object:  StoredProcedure [dbo].[spProcessPayment]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Processing the Payment
CREATE   PROCEDURE [dbo].[spProcessPayment]
    @ReservationID INT,
    @TotalAmount DECIMAL(10,2),
    @PaymentMethod NVARCHAR(50),
    @PaymentID INT OUTPUT,
    @Status BIT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Ensures that if an error occurs, all changes are rolled back

    BEGIN TRY
        BEGIN TRANSACTION

            -- Validate that the reservation exists and the total cost matches
            DECLARE @TotalCost DECIMAL(10,2);
            DECLARE @NumberOfNights INT;
            SELECT @TotalCost = TotalCost, @NumberOfNights = NumberOfNights
            FROM Reservations 
            WHERE ReservationID = @ReservationID;
            
            IF @TotalCost IS NULL
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Reservation does not exist.';
                RETURN;
            END

            IF @TotalAmount <> @TotalCost
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Input total amount does not match the reservation total cost.';
                RETURN;
            END

            -- Calculate Base Amount and GST, assuming GST as 18% for the Payments table
            DECLARE @BaseAmount DECIMAL(10,2) = @TotalCost / 1.18; 
            DECLARE @GST DECIMAL(10,2) = @TotalCost - @BaseAmount;

            -- Insert into Payments Table
            INSERT INTO Payments (ReservationID, Amount, GST, TotalAmount, PaymentDate, PaymentMethod, PaymentStatus)
            VALUES (@ReservationID, @BaseAmount, @GST, @TotalCost, GETDATE(), @PaymentMethod, 'Pending');

            SET @PaymentID = SCOPE_IDENTITY(); -- Capture the new Payment ID

            -- Insert into PaymentDetails table for each room with number of nights and calculated amounts
            INSERT INTO PaymentDetails (PaymentID, ReservationRoomID, Amount, NumberOfNights, GST, TotalAmount)
            SELECT @PaymentID, rr.ReservationRoomID, r.Price, @NumberOfNights, (r.Price * @NumberOfNights) * 0.18, r.Price * @NumberOfNights + (r.Price * @NumberOfNights) * 0.18
            FROM ReservationRooms rr
            JOIN Rooms r ON rr.RoomID = r.RoomID
            WHERE rr.ReservationID = @ReservationID;

            SET @Status = 1; -- Success
            SET @Message = 'Payment Processed Successfully.';
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spProcessRefund]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Process Refund
-- Processes refunds for approved cancellations.
CREATE   PROCEDURE [dbo].[spProcessRefund]
    @CancellationRequestID INT,
    @ProcessedByUserID INT,
    @RefundMethodID INT,
    @RefundID INT OUTPUT,  -- Output parameter for the newly created Refund ID
    @Status BIT OUTPUT,   -- Output parameter for operation status
    @Message NVARCHAR(255) OUTPUT  -- Output parameter for operation message
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.
    DECLARE @PaymentID INT, @RefundAmount DECIMAL(10,2), @CancellationCharge DECIMAL(10,2), @NetRefundAmount DECIMAL(10,2);

    BEGIN TRY
        BEGIN TRANSACTION

            -- Validate the existence of the CancellationRequestID and its approval status
            IF NOT EXISTS (SELECT 1 FROM CancellationRequests 
                           WHERE CancellationRequestID = @CancellationRequestID AND Status = 'Approved')
            BEGIN
                SET @Status = 0;  -- Failure
                SET @Message = 'Invalid CancellationRequestID or the request has not been approved.';
                RETURN;
            END

            -- Retrieve the total amount and cancellation charge from the CancellationCharges table
            SELECT 
                @PaymentID = p.PaymentID,
                @RefundAmount = cc.TotalCost,
                @CancellationCharge = cc.CancellationCharge
            FROM CancellationCharges cc
            JOIN Payments p ON p.ReservationID = (SELECT ReservationID FROM CancellationRequests WHERE CancellationRequestID = @CancellationRequestID)
            WHERE cc.CancellationRequestID = @CancellationRequestID;

            -- Calculate the net refund amount after deducting the cancellation charge
            SET @NetRefundAmount = @RefundAmount - @CancellationCharge;

            -- Insert into Refunds table, mark the Refund Status as Pending
            INSERT INTO Refunds (PaymentID, RefundAmount, RefundDate, RefundReason, RefundMethodID, ProcessedByUserID, RefundStatus, CancellationCharge, NetRefundAmount, CancellationRequestID)
            VALUES (@PaymentID, @NetRefundAmount, GETDATE(), 'Cancellation Approved', @RefundMethodID, @ProcessedByUserID, 'Pending', @CancellationCharge, @NetRefundAmount, @CancellationRequestID);

            -- Capture the newly created Refund ID
            SET @RefundID = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
        SET @Status = 1;  -- Success
        SET @Message = 'Refund processed successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0;  -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spReviewCancellationRequest]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Review Cancellation Request
-- This procedure is used by an admin to review and either approve or reject a cancellation request.
CREATE   PROCEDURE [dbo].[spReviewCancellationRequest]
    @CancellationRequestID INT,
    @AdminUserID INT,
    @ApprovalStatus NVARCHAR(50),  -- 'Approved' or 'Rejected'
    @Status BIT OUTPUT,  -- Output parameter for operation status
    @Message NVARCHAR(255) OUTPUT  -- Output parameter for operation message
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.
    DECLARE @ReservationID INT, @CalcStatus BIT, @CalcMessage NVARCHAR(MAX);
    DECLARE @RoomsCancelled AS RoomIDTableType;
    DECLARE @CalcTotalCost DECIMAL(10,2), @CalcCancellationCharge DECIMAL(10,2),
            @CalcCancellationPercentage DECIMAL(10,2), @CalcPolicyDescription NVARCHAR(255);

    BEGIN TRY
        -- Validate the existence of the Cancellation Request
        IF NOT EXISTS (SELECT 1 FROM CancellationRequests WHERE CancellationRequestID = @CancellationRequestID)
        BEGIN
            SET @Status = 0;  -- Failure
            SET @Message = 'Cancellation request does not exist.';
            RETURN;
        END

        -- Validate the Admin User exists and is active
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @AdminUserID AND IsActive = 1)
        BEGIN
            SET @Status = 0;  -- Failure
            SET @Message = 'Admin user does not exist or is not active.';
            RETURN;
        END

        -- Validate the Approval Status
        IF @ApprovalStatus NOT IN ('Approved', 'Rejected')
        BEGIN
            SET @Status = 0;  -- Failure
            SET @Message = 'Invalid approval status.';
            RETURN;
        END

        BEGIN TRANSACTION
            -- Update the Cancellation Requests
            UPDATE CancellationRequests
            SET Status = @ApprovalStatus, AdminReviewedByID = @AdminUserID, ReviewDate = GETDATE()
            WHERE CancellationRequestID = @CancellationRequestID;

            SELECT @ReservationID = ReservationID FROM CancellationRequests WHERE CancellationRequestID = @CancellationRequestID;

            IF @ApprovalStatus = 'Approved'
            BEGIN
                -- Fetch all rooms associated with the cancellation request
                INSERT INTO @RoomsCancelled (RoomID)
                SELECT rr.RoomID
                FROM CancellationDetails cd
                JOIN ReservationRooms rr ON cd.ReservationRoomID = rr.ReservationRoomID
                WHERE cd.CancellationRequestID = @CancellationRequestID;

                -- Call the calculation procedure
                EXEC spCalculateCancellationCharges 
                    @ReservationID = @ReservationID,
                    @RoomsCancelled = @RoomsCancelled,
                    @TotalCost = @CalcTotalCost OUTPUT,
                    @CancellationCharge = @CalcCancellationCharge OUTPUT,
                    @CancellationPercentage = @CalcCancellationPercentage OUTPUT,
                    @PolicyDescription = @CalcPolicyDescription OUTPUT,
                    @Status = @CalcStatus OUTPUT,
                    @Message = @CalcMessage OUTPUT;

                IF @CalcStatus = 0  -- Check if the charge calculation was unsuccessful
                BEGIN
                    SET @Status = 0;  -- Failure
                    SET @Message = 'Failed to calculate cancellation charges: ' + @CalcMessage;
                    ROLLBACK TRANSACTION;
                    RETURN;
                END

                -- Insert into CancellationCharges table
                INSERT INTO CancellationCharges (CancellationRequestID, TotalCost, CancellationCharge, CancellationPercentage, PolicyDescription)
                VALUES (@CancellationRequestID, @CalcTotalCost, @CalcCancellationCharge, @CalcCancellationPercentage, @CalcPolicyDescription);

                UPDATE Rooms
                SET Status = 'Available'
                WHERE RoomID IN (SELECT RoomID FROM @RoomsCancelled);

                UPDATE Reservations
                SET Status = CASE 
                                 WHEN (SELECT COUNT(*) FROM ReservationRooms WHERE ReservationID = @ReservationID) = 
                                      (SELECT COUNT(*) FROM @RoomsCancelled)
                                 THEN 'Cancelled'
                                 ELSE 'Partially Cancelled'
                             END
                WHERE ReservationID = @ReservationID;
            END

        COMMIT TRANSACTION;
        SET @Status = 1;  -- Success
        SET @Message = 'Cancellation request handled successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0;  -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spSearchByAmenities]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by Amenities
-- Searches rooms offering a specific amenity.
-- Inputs: @AmenityName - Name of the amenity
-- Returns: List of rooms offering the specified amenity along with their type details
CREATE   PROCEDURE [dbo].[spSearchByAmenities]
@AmenityName NVARCHAR(100)
AS
BEGIN
SET NOCOUNT ON;
SELECT DISTINCT r.RoomID, r.RoomNumber, r.RoomTypeID, r.Price, r.BedType, r.ViewType, r.Status,
rt.TypeName, rt.AccessibilityFeatures, rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
JOIN RoomAmenities ra ON rt.RoomTypeID = ra.RoomTypeID
JOIN Amenities a ON ra.AmenityID = a.AmenityID
WHERE a.Name = @AmenityName
AND r.IsActive = 1
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchByAvailability]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by Availability Dates
-- Searches rooms that are available between specified check-in and check-out dates.
-- Inputs: @CheckInDate - Desired check-in date, @CheckOutDate - Desired check-out date
-- Returns: List of rooms that are available along with their type details
CREATE   PROCEDURE [dbo].[spSearchByAvailability]
    @CheckInDate DATE,
    @CheckOutDate DATE
AS
BEGIN
    SET NOCOUNT ON; -- Suppresses the 'rows affected' message

    -- Select rooms that are not currently booked for the given date range and not under maintenance
    SELECT r.RoomID, r.RoomNumber, r.RoomTypeID, r.Price, r.BedType, r.ViewType, r.Status,
           rt.TypeName, rt.AccessibilityFeatures, rt.Description
    FROM Rooms r
    JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
    LEFT JOIN ReservationRooms rr ON rr.RoomID = r.RoomID
    LEFT JOIN Reservations res ON rr.ReservationID = res.ReservationID 
        AND res.Status NOT IN ('Cancelled')
        AND (
            (res.CheckInDate <= @CheckOutDate AND res.CheckOutDate >= @CheckInDate)
        )
    WHERE res.ReservationID IS NULL AND r.Status = 'Available' AND r.IsActive = 1
END;
GO
/****** Object:  StoredProcedure [dbo].[spSearchByMinRating]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by Rating
-- Searches rooms based on a minimum average guest rating.
-- Inputs: @MinRating - Minimum average rating required
-- Searches rooms based on a minimum average guest rating.
-- Inputs: @MinRating - Minimum average rating required
CREATE   PROCEDURE [dbo].[spSearchByMinRating]
@MinRating FLOAT
AS
BEGIN
SET NOCOUNT ON;
-- A subquery to calculate average ratings for each room via their reservations
WITH RatedRooms AS (
SELECT 
res.RoomID,
AVG(CAST(fb.Rating AS FLOAT)) AS AvgRating  -- Calculate average rating per room
FROM Feedbacks fb
JOIN Reservations res ON fb.ReservationID = res.ReservationID
GROUP BY res.RoomID
HAVING AVG(CAST(fb.Rating AS FLOAT)) >= @MinRating  -- Filter rooms by minimum rating
)
SELECT 
r.RoomID, 
r.RoomNumber, 
r.Price, 
r.BedType, 
r.ViewType, 
r.Status,
rt.RoomTypeID, 
rt.TypeName, 
rt.AccessibilityFeatures, 
rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
JOIN RatedRooms rr ON r.RoomID = rr.RoomID  -- Join with the subquery of rated rooms
WHERE r.IsActive = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchByPriceRange]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by Price Range
-- Searches rooms within a specified price range.
-- Inputs: @MinPrice - Minimum room price, @MaxPrice - Maximum room price
-- Returns: List of rooms within the price range along with their type details
CREATE   PROCEDURE [dbo].[spSearchByPriceRange]
@MinPrice DECIMAL(10,2),
@MaxPrice DECIMAL(10,2)
AS
BEGIN
SET NOCOUNT ON; -- Avoids sending row count information
SELECT r.RoomID, r.RoomNumber, r.Price, r.BedType, r.ViewType, r.Status,
rt.RoomTypeID, rt.TypeName, rt.AccessibilityFeatures, rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
WHERE r.Price BETWEEN @MinPrice AND @MaxPrice
AND r.IsActive = 1 AND rt.IsActive = 1
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchByRoomType]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by Room Type
-- Searches rooms based on room type name.
-- Inputs: @RoomTypeName - Name of the room type
-- Returns: List of rooms matching the room type name along with type details
CREATE   PROCEDURE [dbo].[spSearchByRoomType]
@RoomTypeName NVARCHAR(50)
AS
BEGIN
SET NOCOUNT ON;
SELECT r.RoomID, r.RoomNumber, r.Price, r.BedType, r.ViewType, r.Status,
rt.RoomTypeID, rt.TypeName, rt.AccessibilityFeatures, rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
WHERE rt.TypeName = @RoomTypeName
AND r.IsActive = 1
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchByViewType]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search by View Type
-- Searches rooms by specific view type.
-- Inputs: @ViewType - Type of view from the room (e.g., sea, city)
-- Returns: List of rooms with the specified view along with their type details
CREATE   PROCEDURE [dbo].[spSearchByViewType]
@ViewType NVARCHAR(50)
AS
BEGIN
SET NOCOUNT ON;
SELECT r.RoomID, r.RoomNumber, r.RoomTypeID, r.Price, r.BedType, r.Status, r.ViewType,
rt.TypeName, rt.AccessibilityFeatures, rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
WHERE r.ViewType = @ViewType
AND r.IsActive = 1
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchCustomCombination]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Custom Combination Searches with Dynamic SQL
-- Searches for rooms based on a combination of criteria including price range, room type, and amenities.
-- Inputs:
-- @MinPrice DECIMAL(10,2) = NULL: Minimum price filter (optional)
-- @MaxPrice DECIMAL(10,2) = NULL: Maximum price filter (optional)
-- @RoomTypeName NVARCHAR(50) = NULL: Room type Name filter (optional)
-- @AmenityName NVARCHAR(100) = NULL: Amenity Name filter (optional)
-- @@ViewType NVARCHAR(50) = NULL: View Type filter (optional)
-- Returns: List of rooms matching the combination of specified criteria along with their type details
-- Note: Based on the Requirements you can use AND or OR Conditions
CREATE   PROCEDURE [dbo].[spSearchCustomCombination]
@MinPrice DECIMAL(10,2) = NULL,
@MaxPrice DECIMAL(10,2) = NULL,
@RoomTypeName NVARCHAR(50) = NULL,
@AmenityName NVARCHAR(100) = NULL,
@ViewType NVARCHAR(50) = NULL
AS
BEGIN
SET NOCOUNT ON; -- Suppresses the 'rows affected' message
DECLARE @SQL NVARCHAR(MAX)
SET @SQL = 'SELECT DISTINCT r.RoomID, r.RoomNumber, r.Price, r.BedType, r.ViewType, r.Status, 
rt.RoomTypeID, rt.TypeName, rt.AccessibilityFeatures, rt.Description 
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
LEFT JOIN RoomAmenities ra ON rt.RoomTypeID = ra.RoomTypeID
LEFT JOIN Amenities a ON ra.AmenityID = a.AmenityID
WHERE r.IsActive = 1 '
DECLARE @Conditions NVARCHAR(MAX) = ''
-- Dynamic conditions based on input parameters
IF @MinPrice IS NOT NULL
SET @Conditions = @Conditions + 'AND r.Price >= @MinPrice '
IF @MaxPrice IS NOT NULL
SET @Conditions = @Conditions + 'AND r.Price <= @MaxPrice '
IF @RoomTypeName IS NOT NULL
SET @Conditions = @Conditions + 'AND rt.TypeName LIKE ''%' + @RoomTypeName + '%'' '
IF @AmenityName IS NOT NULL
SET @Conditions = @Conditions + 'AND a.Name LIKE ''%' + @AmenityName + '%'' '
IF @ViewType IS NOT NULL
SET @Conditions = @Conditions + 'AND r.ViewType = @ViewType '
-- Remove the first OR if any conditions were added
IF LEN(@Conditions) > 0
SET @SQL = @SQL + ' AND (' + STUFF(@Conditions, 1, 3, '') + ')'
-- Execute the dynamic SQL
EXEC sp_executesql @SQL,
N'@MinPrice DECIMAL(10,2), @MaxPrice DECIMAL(10,2), @RoomTypeName NVARCHAR(50), @AmenityName NVARCHAR(100), @ViewType NVARCHAR(50)',
@MinPrice, @MaxPrice, @RoomTypeName, @AmenityName, @ViewType
END
GO
/****** Object:  StoredProcedure [dbo].[spSearchRoomsByRoomTypeID]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Search All Rooms by RoomTypeID
-- Searches all rooms based on a specific RoomTypeID.
-- Inputs: @RoomTypeID - The ID of the room type
-- Returns: List of all rooms associated with the specified RoomTypeID along with type details
CREATE   PROCEDURE [dbo].[spSearchRoomsByRoomTypeID]
@RoomTypeID INT
AS
BEGIN
SET NOCOUNT ON;
SELECT r.RoomID, r.RoomNumber, r.Price, r.BedType, r.ViewType, r.Status,
rt.RoomTypeID, rt.TypeName, rt.AccessibilityFeatures, rt.Description
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
WHERE rt.RoomTypeID = @RoomTypeID
AND r.IsActive = 1
END
GO
/****** Object:  StoredProcedure [dbo].[spToggleRoomTypeActive]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Activate/Deactivate RoomType
CREATE PROCEDURE [dbo].[spToggleRoomTypeActive]
    @RoomTypeID INT,
    @IsActive BIT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check user existence
        IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
        BEGIN
             SET @StatusCode = 1 -- Failure due to not found
             SET @Message = 'Room type not found.'
        END

        -- Update IsActive status
        BEGIN TRANSACTION
             UPDATE RoomTypes SET IsActive = @IsActive WHERE RoomTypeID = @RoomTypeID;
                SET @StatusCode = 0 -- Success
             SET @Message = 'Room type activated/deactivated successfully.'
        COMMIT TRANSACTION

    END TRY
    -- Handle exceptions
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER() -- SQL Server error number
        SET @Message = ERROR_MESSAGE()
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spToggleUserActive]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Activate/Deactivate User
-- This can also be used for deleting a User
CREATE PROCEDURE [dbo].[spToggleUserActive]
    @UserID INT,
    @IsActive BIT,
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check user existence
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
        BEGIN
            SET @ErrorMessage = 'User not found.';
            RETURN;
        END

        -- Update IsActive status
        BEGIN TRANSACTION
            UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserID;
        COMMIT TRANSACTION

        SET @ErrorMessage = NULL;
    END TRY
    -- Handle exceptions
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spUpdateAmenity]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Description: Updates an existing amenity's details in the Amenities table.
-- Checks if the amenity exists before attempting an update.
CREATE   PROCEDURE [dbo].[spUpdateAmenity]
@AmenityID INT,
@Name NVARCHAR(100),
@Description NVARCHAR(255),
@IsActive BIT,
@ModifiedBy NVARCHAR(100),
@Status BIT OUTPUT,
@Message NVARCHAR(255) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION
-- Check if the amenity exists before updating.
IF NOT EXISTS (SELECT 1 FROM Amenities WHERE AmenityID = @AmenityID)
BEGIN
SET @Status = 0;
SET @Message = 'Amenity does not exist.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Check for name uniqueness excluding the current amenity.
IF EXISTS (SELECT 1 FROM Amenities WHERE Name = @Name AND AmenityID <> @AmenityID)
BEGIN
SET @Status = 0;
SET @Message = 'The name already exists for another amenity.';
ROLLBACK TRANSACTION;
RETURN;
END
-- Update the amenity details.
UPDATE Amenities
SET Name = @Name, Description = @Description, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
WHERE AmenityID = @AmenityID;
-- Check if the update was successful
IF @@ROWCOUNT = 0
BEGIN
SET @Status = 0;
SET @Message = 'No records updated.';
ROLLBACK TRANSACTION;
END
ELSE
BEGIN
SET @Status = 1;
SET @Message = 'Amenity updated successfully.';
COMMIT TRANSACTION;
END
END TRY
BEGIN CATCH
-- Handle exceptions and roll back the transaction if an error occurs.
ROLLBACK TRANSACTION;
SET @Status = 0;
SET @Message = ERROR_MESSAGE();
END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[spUpdatePaymentStatus]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure for Updating the Payment Status
CREATE   PROCEDURE [dbo].[spUpdatePaymentStatus]
    @PaymentID INT,
    @NewStatus NVARCHAR(50), -- 'Completed' or 'Failed'
    @FailureReason NVARCHAR(255) = NULL, -- Optional reason for failure
    @Status BIT OUTPUT, -- Output to indicate success/failure of the procedure
    @Message NVARCHAR(255) OUTPUT -- Output message detailing the result
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Ensure that if an error occurs, all changes are rolled back

    BEGIN TRY
        BEGIN TRANSACTION
            -- Check if the payment exists and is in a 'Pending' status
            DECLARE @CurrentStatus NVARCHAR(50);
            SELECT @CurrentStatus = PaymentStatus FROM Payments WHERE PaymentID = @PaymentID;
            
            IF @CurrentStatus IS NULL
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Payment record does not exist.';
                RETURN;
            END

            IF @CurrentStatus <> 'Pending'
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Payment status is not Pending. Cannot update.';
                RETURN;
            END

            -- Validate the new status
            IF @NewStatus NOT IN ('Completed', 'Failed')
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Invalid status value. Only "Completed" or "Failed" are acceptable.';
                RETURN;
            END

            -- Update the Payment Status
            UPDATE Payments
            SET PaymentStatus = @NewStatus,
                FailureReason = CASE WHEN @NewStatus = 'Failed' THEN @FailureReason ELSE NULL END
            WHERE PaymentID = @PaymentID;

            -- If Payment Fails, update corresponding reservation and room statuses
            IF @NewStatus = 'Failed'
            BEGIN
                DECLARE @ReservationID INT;
                SELECT @ReservationID = ReservationID FROM Payments WHERE PaymentID = @PaymentID;

                -- Update Reservation Status
                UPDATE Reservations
                SET Status = 'Cancelled'
                WHERE ReservationID = @ReservationID;

                -- Update Room Status
                UPDATE Rooms
                SET Status = 'Available'
                FROM Rooms
                JOIN ReservationRooms ON Rooms.RoomID = ReservationRooms.RoomID
                WHERE ReservationRooms.ReservationID = @ReservationID;
            END

            SET @Status = 1; -- Success
            SET @Message = 'Payment Status Updated Successfully.';
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spUpdateRefundStatus]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Update Refund Status
CREATE   PROCEDURE [dbo].[spUpdateRefundStatus]
    @RefundID INT,
    @NewRefundStatus NVARCHAR(50),
    @Status BIT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Automatically roll-back the transaction on error.

    -- Define valid statuses, adjust these as necessary for your application
    DECLARE @ValidStatuses TABLE (Status NVARCHAR(50));
    INSERT INTO @ValidStatuses VALUES ('Pending'), ('Processed'), ('Completed'), ('Failed');

    BEGIN TRY
        BEGIN TRANSACTION
            -- Check current status of the refund to avoid updating final states like 'Completed'
            DECLARE @CurrentStatus NVARCHAR(50);
            SELECT @CurrentStatus = RefundStatus FROM Refunds WHERE RefundID = @RefundID;

            IF @CurrentStatus IS NULL
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Refund not found.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

            IF @CurrentStatus = 'Completed'
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Refund is already completed and cannot be updated.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

            -- Validate the new refund status
            IF NOT EXISTS (SELECT 1 FROM @ValidStatuses WHERE Status = @NewRefundStatus)
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'Invalid new refund status provided.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

            -- Update the Refund Status if validations pass
            UPDATE Refunds
            SET RefundStatus = @NewRefundStatus
            WHERE RefundID = @RefundID;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @Status = 0; -- Failure
                SET @Message = 'No refund found with the provided RefundID.';
                ROLLBACK TRANSACTION;
                RETURN;
            END

        COMMIT TRANSACTION;
        SET @Status = 1; -- Success
        SET @Message = 'Refund status updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Status = 0; -- Failure
        SET @Message = ERROR_MESSAGE();
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[spUpdateRoom]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Update Room
CREATE   PROCEDURE [dbo].[spUpdateRoom]
    @RoomID INT,
    @RoomNumber NVARCHAR(10),
    @RoomTypeID INT,
    @Price DECIMAL(10,2),
    @BedType NVARCHAR(50),
    @ViewType NVARCHAR(50),
    @Status NVARCHAR(50),
    @IsActive BIT,
    @ModifiedBy NVARCHAR(100),
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            -- Check if the RoomTypeID is valid and room number is unique for other rooms
            IF EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID) AND
               NOT EXISTS (SELECT 1 FROM Rooms WHERE RoomNumber = @RoomNumber AND RoomID <> @RoomID)
            BEGIN
                -- Verify the room exists before updating
                IF EXISTS (SELECT 1 FROM Rooms WHERE RoomID = @RoomID)
                BEGIN
                    UPDATE Rooms
                    SET RoomNumber = @RoomNumber,
                        RoomTypeID = @RoomTypeID,
                        Price = @Price,
                        BedType = @BedType,
                        ViewType = @ViewType,
                        Status = @Status,
                        IsActive = @IsActive,
                        ModifiedBy = @ModifiedBy,
                        ModifiedDate = GETDATE()
                    WHERE RoomID = @RoomID

                    SET @StatusCode = 0 -- Success
                    SET @Message = 'Room updated successfully.'
                END
                ELSE
                BEGIN
                    SET @StatusCode = 2 -- Failure due to room not found
                    SET @Message = 'Room not found.'
                END
            END
            ELSE
            BEGIN
                SET @StatusCode = 1 -- Failure due to invalid RoomTypeID or duplicate room number
                SET @Message = 'Invalid Room Type ID or duplicate room number.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER()
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spUpdateRoomType]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Update Room Type
CREATE PROCEDURE [dbo].[spUpdateRoomType]
    @RoomTypeID INT,
    @TypeName NVARCHAR(50),
    @AccessibilityFeatures NVARCHAR(255),
    @Description NVARCHAR(255),
    @ModifiedBy NVARCHAR(100),
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION
            -- Check if the updated type name already exists in another record
            IF NOT EXISTS (SELECT 1 FROM RoomTypes WHERE TypeName = @TypeName AND RoomTypeID <> @RoomTypeID)
            BEGIN
                IF EXISTS (SELECT 1 FROM RoomTypes WHERE RoomTypeID = @RoomTypeID)
                BEGIN
                    UPDATE RoomTypes
                    SET TypeName = @TypeName,
                        AccessibilityFeatures = @AccessibilityFeatures,
                        Description = @Description,
                        ModifiedBy = @ModifiedBy,
                        ModifiedDate = GETDATE()
                    WHERE RoomTypeID = @RoomTypeID

                    SET @StatusCode = 0 -- Success
                    SET @Message = 'Room type updated successfully.'
                END
                ELSE
                BEGIN
                    SET @StatusCode = 2 -- Failure due to not found
                    SET @Message = 'Room type not found.'
                END
            END
            ELSE
            BEGIN
                SET @StatusCode = 1 -- Failure due to duplicate name
                SET @Message = 'Another room type with the same name already exists.'
            END
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @StatusCode = ERROR_NUMBER() -- SQL Server error number
        SET @Message = ERROR_MESSAGE()
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spUpdateUserInformation]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Update User Information
CREATE PROCEDURE [dbo].[spUpdateUserInformation]
    @UserID INT,
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @ModifiedBy NVARCHAR(100),
    @ErrorMessage NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Check user existence
        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
        BEGIN
            SET @ErrorMessage = 'User not found.';
            RETURN;
        END

        -- Check email uniqueness except for the current user
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND UserID <> @UserID)
        BEGIN
            SET @ErrorMessage = 'Email already used by another user.';
            RETURN;
        END

        -- Update user details
        BEGIN TRANSACTION
            UPDATE Users
            SET Email = @Email, PasswordHash =@Password, ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
            WHERE UserID = @UserID;
        COMMIT TRANSACTION

        SET @ErrorMessage = NULL;
    END TRY
    -- Handle exceptions
    BEGIN CATCH
        ROLLBACK TRANSACTION
        SET @ErrorMessage = ERROR_MESSAGE();
    END CATCH
END;
GO
