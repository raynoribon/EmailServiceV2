using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailService;

namespace EmailReceiverService
{
    public class Receiver
    {
        public List<string> Servers = new List<string>();
        public bool Quit = false;
        public bool IsBusy = false;
        public string TempFolder = "";
        public string Message = "";
        public Int32 CheckInterval = 60000; //1 min or 60 sec
        //public Int32 CheckInterval = 300000; // 5 mins

        public DB DB = new DB();

        public string ClientId = "41b25438-eb33-47d2-a2a2-d46d5767b2b0";
        public string ClientSecret = "qhT8Q~WyJ_2lNS-RNF5a2PfwGxGhIj9MTDW5GaRg";

        public Receiver()
        {
        }

        public void Start()
        {

            while (!this.Quit)
            {
                this.IsBusy = true;
                this.Process();
                Common.ReceiverLog("Receiver Sleep for 1 sec");
                System.Threading.Thread.Sleep(this.CheckInterval);
                this.IsBusy = false;
            }
        }


        public void Process()
        {
            try
            {
                string result = "";
                Common.ReceiverLog("Get Credential Emails for log.in");
                //string sql = @"SELECT * FROM MENA_email_server WHERE category_id = 42";
                //string sql = @"SELECT * FROM MENA_email_server WHERE server_id = 8"; //porsche only                
                //string sql = @"select * from MENA_email_server where server_id = 10"; //Testing for Maserati only
                //string sql = @"select * from MENA_email_server where server_id = 4"; //ford only
                //string sql = @"SELECT * FROM MENA_email_server WHERE category_id IN (4)";
                string sql = @"SELECT * FROM MENA_email_server WHERE category_id = 32"; // all CRC Emails
                DataTable tblServers = DB.ExecSQL(sql, out result);
                //DataTable tblServers = new DataTable();
                Common.ReceiverLog("Found " + tblServers.Rows.Count + " credentials");
                foreach (DataRow rowServer in tblServers.Rows)
                {
                    bool isInternal = false;
                    bool.TryParse(rowServer["server_internal"].ToString(), out isInternal);

                    bool deleteEmail = false;
                    bool.TryParse(Common.GetConfig("Delete Email"), out deleteEmail);

                    string userName = rowServer["server_username"].ToString();
                    EmailServices sv = new EmailServices(ClientId, ClientSecret);
                    List<EmailService._email> emails = sv.GetEmailByUserName(userName);

                    if (result == "" || emails.Count > 0)
                    {
                        //throw new Exception(result);

                        Common.ReceiverLog("Receiver > reading from " + rowServer["server_friendlyname"].ToString());

                        foreach (EmailService._email email in emails)
                        {
                            Int32 attachCount = 0;
                            result = this.SaveToInbox(email, rowServer["server_id"].ToString());
                            if (result != "" && result != "Receiver.SaveToInbox > Exception > record already exist") throw new Exception(result);

                            if (result == "")
                            {
                                string inboxId = DB.GetValue("SELECT MAX(inbox_id) FROM MENA_email_inbox WHERE inbox_signature='" + email.ID + "'", 0, "0");
                                if (email.Attachments != null)
                                {
                                    foreach (EmailService._attachment attachment in email.Attachments)
                                    {
                                        result = this.SaveToAttachment(attachment, "inbox", inboxId);
                                        if (result != "") throw new Exception(result);
                                        attachCount++;
                                    }
                                }
                                Common.ReceiverLog("Receiver > Email " + email.Subject + " saved to inbox with " + attachCount.ToString() + " attachment(s)");

                                //Moving Emails to Archive                                    
                                bool move = sv.MoveEmailById(userName, "Archive", email.ID);
                                if (!move)
                                {
                                    Common.ReceiverLog("Receiver > Email " + email.Subject + " was not successfully moved to Archive ");
                                }
                                else
                                {
                                    Common.ReceiverLog("Receiver > Email " + email.Subject + " was successfully moved to Archive ");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Common.ReceiverLog("Receiver.Process > Exception > " + e.Message);
            }
        }

        private string SaveToInbox(EmailService._email email, string server)
        {
            //MailBuilder builder = new MailBuilder();

            try
            {
                string result = "";
                DateTime _dateTime = Convert.ToDateTime(email.Date);
                string dateString = _dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                //string qry = "SELECT * FROM MENA_email_inbox WHERE inbox_signature='" + email.ID + "' and convert(varchar, inbox_date, 120)=convert(varchar,'" + dateString + "', 120)";
                string qry = string.Format("app_query_crc_Email_Inbox_CheckExistingEmail '{0}','{1}'", email.Uid, dateString); //create an SP for easier modification .. 10-14-2020 ..

                DataTable inbox = DB.ExecSQL(qry, out result);

                if (inbox.Rows.Count <= 0)
                {
                    DataRow r = inbox.NewRow();
                    r["inbox_from"] = Common.Left(email.From, 300);
                    r["inbox_to"] = Common.Left(email.To, 300);
                    r["inbox_cc"] = Common.Left(email.Cc, 300);
                    r["inbox_bcc"] = Common.Left(email.Bcc, 300);
                    r["inbox_subject"] = Common.Left(email.Subject, 540);
                    r["inbox_body"] = email.Body;
                    r["inbox_date"] = email.Date;
                    r["inbox_signature"] = email.ID;
                    r["server_id"] = server;
                    r["created_on"] = DateTime.Now.ToString();
                    r["uid"] = email.Uid;
                    inbox.Rows.Add(r);
                    inbox.TableName = "MENA_email_inbox";
                    result = DB.Update(inbox);
                }
                else
                    result = "record already exist";

                if (result != "") throw new Exception(result);
                return "";
            }
            catch (Exception e)
            {
                Common.ReceiverLog("Receiver.SaveToInbox > Exception > " + e.Message);
                return "Receiver.SaveToInbox > Exception > " + e.Message;
            }
        }


        private string SaveToAttachment(EmailService._attachment attachment, string OwnerName, string OwnerID)
        {
            try
            {
                string result = "";
                DataTable attach = DB.ExecSQL("SELECT * FROM MENA_email_attachment WHERE 1=0", out result);
                //DataTable attach = DB.ExecSQL("SELECT * FROM MENA_email_attachment_Test01 WHERE 1=0", out result);
                if (result != "") throw new Exception(result);
                DataRow row = attach.NewRow();
                row["attachment_name"] = attachment.Filename;
                row["attachment_type"] = attachment.Filetype;
                row["attachment_data"] = attachment.Data;
                row["attachment_guid"] = attachment.GUID;
                row["owner_id"] = OwnerID;
                row["owner_name"] = OwnerName; //always set to inbox since this is only a reciever..
                row["created_on"] = DateTime.Now.ToString();
                attach.Rows.Add(row);
                attach.TableName = "MENA_email_attachment";
                //attach.TableName = "MENA_email_attachment_Test01";               
                result = DB.Update(attach);
                if (result != "") throw new Exception(result);
                return "";
            }
            catch (Exception e)
            {
                Common.ReceiverLog("Receiver.SaveToAttachment > Exception > " + e.Message);
                return "Receiver.SaveToAttachment > Exception > " + e.Message;
            }
        }
    }
}
