
using EmailService;
using System.Data;

namespace FuntionTester
{
    class Program //: ServiceBase
    {       

        static void Main()
        {
            //For Local
#if DEBUG
            string meesageId = "AAMkAGM0N2ZjMTZhLWEyOWMtNDQ0NC05MWE4LTExODFjZGQyODBjMQBGAAAAAACyBUANt_kaTJnOi1NPH7tFBwBQICPqR7B5RaMUPl9dXxS-AAAAAAEMAABQICPqR7B5RaMUPl9dXxS-AAB38qAoAAA=";            

            string ClientId = "41b25438-eb33-47d2-a2a2-d46d5767b2b0";
            string ClientSecret = "qhT8Q~WyJ_2lNS-RNF5a2PfwGxGhIj9MTDW5GaRg";
            EmailServices sv = new EmailServices(ClientId, ClientSecret);

            List<_email> emails = sv.GetEmailByUserName("ford@menaroadassist.com");
            //DataTable dt = sv.GetEmailByUserNameOLD("ford@menaroadassist.com");
            //DataTable dt = sv.GetEmailByUserName("rene.vizconde@menaa.com");
            bool move = sv.MoveEmailById("rene.vizconde@menaa.com", "Archive", meesageId);
         
#else
                        
#endif
        }    
        
    }
}