using Microsoft.Data.SqlClient;

namespace HotelBookingAPI.Extensions
{
    public static class DataReaderExtensions
    {
        public static T GetValueByColumn<T>(this SqlDataReader reader, string columnName)
        {
            //Getting Column Index
            //This line retrieves the zero-based column ordinal (index) based on the column name provided.
            //GetOrdinal throws an exception if the column name specified does not exist in the reader.
            int index = reader.GetOrdinal(columnName);

            //Checking for Null Values
            //Before trying to retrieve the value, the method checks whether the data at the specified column index is DB null using the IsDBNull method.
            //This prevents exceptions that occur when reading null values.
            if (!reader.IsDBNull(index))
            {
                //Returning the Value
                //If the value is not null, it retrieves the value from the reader at the given index and casts it to the type T.
                //This allows the method to be type-safe and handling various data types.
                return (T)reader[index];
            }

            //Handling Null Values
            //If the value in the database is null, the method returns the default value for the type T.
            //The default value depends on what T is; for reference types, it is null,
            //and for value types, it is typically zero or a struct with all zero values.
            return default(T);
        }
    }
}