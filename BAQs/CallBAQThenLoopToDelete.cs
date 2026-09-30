/*
I use this query to delete metrics records reported by Raspberry Pis we have
around our locations. It asks a query which records to purge then loops through
the query results and runs the UD07.DeleteByID method on them.

Requires Assemblies
- Ice.Contracts.BO.DynamicQuery
- Ice.Contracts.BO.UD07

Also referenced in
UDXX\RecordDeletionWithBO.cs
*/

const string QUERY_ID = "PiMetricsToPurge";
int deletedCount = 0;

// Define the BAQ service
this.CallService<Ice.Contracts.DynamicQuerySvcContract>(dynQuery =>
{
    // Check to see if the query service actually exists
    if (dynQuery != null)
    {
        // Get the default parameters and execute the query with them
        var dsParam = dynQuery.GetQueryExecutionParametersByID(QUERY_ID);
        var dsResults = dynQuery.ExecuteByID(QUERY_ID, dsParam);

        // Check to see if it actually resulted in anything
        if (dsResults == null || !dsResults.Tables.Contains("Results"))
        {
            throw new Ice.BLException($"BAQ '{QUERY_ID}' did not return a Results table!");
        }

        if (dsResults.Tables.Count > 0 && dsResults.Tables["Results"] != null)
        {
            // Define the UD07 service
            this.CallService<Ice.Contracts.UD07SvcContract>(ud07Svc =>
            {
                if (ud07Svc != null)
                {
                    // Loop through each row in the query and delete it. No
                    // transaction scope is needed because the DeleteByID method
                    // handles itself. Or so I've been told.
                    foreach (System.Data.DataRow row in dsResults.Tables["Results"].Rows)
                    {
                        // Use the results from the BAQ we ran above to get the
                        // keys we need to delete specific metrics records
                        string key1 = Convert.ToString(row["Metrics_Key1"]);
                        string key2 = Convert.ToString(row["Metrics_Key2"]);
                        string key3 = Convert.ToString(row["Metrics_Key3"]);
                        string key4 = Convert.ToString(row["Metrics_Key4"]);
                        string key5 = Convert.ToString(row["Metrics_Key5"]);

                        ud07Svc.DeleteByID(key1, key2, key3, key4, key5);
                        deletedCount++;
                    }
                }
            });
        }
    }
});
