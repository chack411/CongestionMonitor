using System;

namespace CongestionCameraConsoleApp
{
    public class AppSettings
    {
        public string Face_API_Endpoint { get; set; }
        public string Face_API_Subscription_Key { get; set; }
        public string FaceCountDB_Endpoint { get; set; }
        public string FaceCountDB_Key { get; set; }
        public string DatabaseId { get; set; }
        public string ContainerId { get; set; }
        public string PlaceName { get; set; }
        public int AnalysisIntervalInSeconds { get; set; }
        public int CameraIndex { get; set; }

        public AppSettings()
        {
            DatabaseId = "FaceCount";
            ContainerId = "Result1";
            PlaceName = "Paradise";
            AnalysisIntervalInSeconds = 5;
            CameraIndex = 0;
        }

        public bool IsValid()
        {
            bool valid = true;

            if (String.IsNullOrWhiteSpace(Face_API_Endpoint))
            {
                Console.WriteLine("Need to set 'Settings:Face_API_Endpoint'");
                valid = false;
            }
            else
            {
                Console.WriteLine($"Face API: {Face_API_Endpoint}");
            }

            if (String.IsNullOrWhiteSpace(Face_API_Subscription_Key))
            {
                Console.WriteLine("Need to set 'Settings:Face_API_Subscription_Key'");
                valid = false;
            }

            if (String.IsNullOrWhiteSpace(FaceCountDB_Endpoint))
            {
                Console.WriteLine("Need to set 'Settings:FaceCountDB_Endpoint'");
                valid = false;
            }
            else
            {
                Console.WriteLine($"Face DB: {FaceCountDB_Endpoint}");
            }

            if (String.IsNullOrWhiteSpace(FaceCountDB_Key))
            {
                Console.WriteLine("Need to set 'Settings:FaceCountDB_Key'");
                valid = false;
            }

            if (String.IsNullOrWhiteSpace(DatabaseId))
            {
                Console.WriteLine("Need to set 'Settings:DatabaseId'");
                valid = false;
            }

            if (String.IsNullOrWhiteSpace(ContainerId))
            {
                Console.WriteLine("Need to set 'Settings:ContainerId'");
                valid = false;
            }

            if (String.IsNullOrWhiteSpace(PlaceName))
            {
                Console.WriteLine("Need to set 'Settings:PlaceName'");
                valid = false;
            }
            else
            {
                Console.WriteLine($"Place: {PlaceName}");
            }

            if (AnalysisIntervalInSeconds <= 0 || AnalysisIntervalInSeconds > 86400)
            {
                Console.WriteLine("'AnalysisIntervalInSeconds' must be set to a number within 1 to 86400");
                valid = false;
            }
            else
            {
                Console.WriteLine($"Analysis Interval: {AnalysisIntervalInSeconds}");
            }

            if (CameraIndex < 0)
            {
                Console.WriteLine("'CameraIndex' must be set to a number grater than or equal to 0");
                valid = false;
            }
            else
            {
                Console.WriteLine($"Camera Index: {CameraIndex}");
            }

            return valid;
        }
    }
}
