﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace MailboxTroubleshooter
{
    /// <summary>
    /// <para type="synopsis">Find the Status of a mailbox within the Database Mailbox table</para>
    /// <para type="description">This cmdlet uses StoreQuery to extract the Mailbox Status from within a given Database's mailbox table, this can be useful for troubleshooting.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "MailboxStatus")]
    public class GetMailboxStatus : Cmdlet
    {
        /// <summary>
        /// <para type="description">The MailboxGuid of the user</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Provide a users MailboxGuid value")]
        public string MailboxGuid;

        /// <summary>
        /// <para type="description">The Database where the users mailbox resides</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Provide a database")]
        public string Database;

        /// <summary>
        /// Use to perform pre-cmdlet execution tasks such as import modules or validate data before processing
        /// </summary>
        protected override void BeginProcessing() //Used for initializing resources
        {
            try
            {

                ValidateParameters(); //We want to validate our parameters are set
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Calls the Cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                PowerShell myfirstPowerShell = PowerShell.Create(RunspaceMode.CurrentRunspace);
                string query = String.Format("select * from [{0}].Mailbox where MailboxGuid = '{1}'", this.Database, this.MailboxGuid);

                // We add the command we want to run.
                // More information on the PowerShell.AddCommand method can be found here: https://msdn.microsoft.com/en-us/library/dd182430(v=vs.85).aspx
                myfirstPowerShell.AddCommand("Get-StoreQuery");

                // We add the parameter that we want to specify.
                // More information on the PowerShell.AddParameter method can be found here: https://msdn.microsoft.com/en-us/library/dd182434(v=vs.85).aspx

                myfirstPowerShell.AddParameter("Database", this.Database).AddArgument(query);

                // We add the command we want to run.
                // More information on the PowerShell.AddCommand method can be found here: https://msdn.microsoft.com/en-us/library/dd182430(v=vs.85).aspx
                myfirstPowerShell.AddCommand("Select-Object");

                // We select the specific property we want to return.
                // More information on the PowerShell.AddParameter method can be found here: https://msdn.microsoft.com/en-us/library/dd182434(v=vs.85).aspx
                myfirstPowerShell.AddParameter("Property", new string[] { "MailboxGuid", "DisplayName", "DeletedOn", "Status" });

                //Invoke the Powershell Cmdlet and store in a collection
                Collection<PSObject> resultsCollection = myfirstPowerShell.Invoke();

                //Store results from the collection into a PSObject so we can output to the pipeline
                PSObject myFirstPsObject = resultsCollection.FirstOrDefault();

                //Check if we have have results
                if (myFirstPsObject != null)
                {
                    //We setup a new PSObject to hold values we're interested in
                    PSObject psObj = new PSObject();

                    psObj.Members.Add(new PSNoteProperty("DisplayName", myFirstPsObject.Members["DisplayName"].Value.ToString()));
                    psObj.Members.Add(new PSNoteProperty("MailboxGuid", myFirstPsObject.Members["MailboxGuid"].Value.ToString()));
                    psObj.Members.Add(new PSNoteProperty("DeletedOn", myFirstPsObject.Members["DeletedOn"].Value.ToString()));

                    //Lets get the value of the Mailbox Status and store as an Int
                    int MailboxStatus = Int32.Parse(myFirstPsObject.Members["Status"].Value.ToString());

                    //Based on the value of Mailbox Status we add the corresponding readable information to the PSObject.
                    switch (MailboxStatus)
                    {
                        case 0:
                            psObj.Members.Add(new PSNoteProperty("Status", "Invalid"));
                            break;
                        case 1:
                            psObj.Members.Add(new PSNoteProperty("Status", "New"));
                            break;
                        case 2:
                            psObj.Members.Add(new PSNoteProperty("Status", "UserAccessible"));
                            break;
                        case 3:
                            psObj.Members.Add(new PSNoteProperty("Status", "Disabled"));
                            break;
                        case 4:
                            psObj.Members.Add(new PSNoteProperty("Status", "SoftDeleted"));
                            break;
                        case 5:
                            psObj.Members.Add(new PSNoteProperty("Status", "HardDeleted"));
                            break;
                        case 6:
                            psObj.Members.Add(new PSNoteProperty("Status", "Tombstone"));
                            break;
                        case 7:
                            psObj.Members.Add(new PSNoteProperty("Status", "KeyAccessDenied"));
                            break;
                        default:
                            psObj.Members.Add(new PSNoteProperty("Status", "NULL"));
                            break;
                    }

                    //Write out the new PSObject to the pipeline
                    WriteObject(psObj);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Called after ProcessRecord completes
        /// </summary>
        protected override void EndProcessing()
        {

        }

        /// <summary>
        /// Called if the ProcessRecord is interupted by the user, use to cleanup resources such as DB connections etc
        /// </summary>
        protected override void StopProcessing()
        {
        }

        /// <summary>
        /// Validates Cmdlet parameters prior to processing the record
        /// </summary>
        private void ValidateParameters()
        {
            try
            {
                if (String.IsNullOrEmpty(MailboxGuid))
                    ThrowParameterError("MailboxGuid");

                if (String.IsNullOrEmpty(Database))
                    ThrowParameterError("Database");
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void ThrowParameterError(string parameterName)
        {
            ThrowTerminatingError(
                new ErrorRecord(
                    new ArgumentException(String.Format(
                        "Must pecifify '{0}'", parameterName)),
                    Guid.NewGuid().ToString(),
                    ErrorCategory.InvalidArgument,
                    null));
        }
    }
}