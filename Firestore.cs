using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace DAL1
{
    [FirestoreData]
    public class Canteen
    {
        public string CanteenId { get; set; }
        [FirestoreProperty("name")]
        public string Name { get; set; }
    }
    [FirestoreData]
    public class User
    {
        public string UserId { get; set; }
        [FirestoreProperty("vkid")]
        public long? Vkid { get; set; }
        [FirestoreProperty("card")]
        public int Card { get; set; }
        [FirestoreProperty("canteen")]
        public Canteen Canteen { get; set; }
    }
    public class DAL
    {
        string projectId = "eatsibfu";
        FirestoreDb firestoreDb;
        public DAL()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"eatsibfu-firebase-adminsdk-64xrh-30f1f32c6c.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

            firestoreDb = FirestoreDb.Create(projectId);

        }

        public async Task<List<Canteen>> GetCanteen()
        {
            Query canteenQuery = firestoreDb.Collection("canteens");
            QuerySnapshot snapshots = await canteenQuery.GetSnapshotAsync();
            List<Canteen> listCanteens = new List<Canteen>();
            foreach (DocumentSnapshot documentSnapshot in snapshots.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Dictionary<string, object> canteen = documentSnapshot.ToDictionary();
                    string json = JsonConvert.SerializeObject(canteen);
                    Canteen newCanteen = JsonConvert.DeserializeObject<Canteen>(json);
                    newCanteen.CanteenId = documentSnapshot.Id;
                    listCanteens.Add(newCanteen);
                }
            }
            return listCanteens;
        }

        public async Task<DocumentReference> AddUser(long? pearId)
        {

            User user = new User() { Vkid = pearId };
            CollectionReference colRef = firestoreDb.Collection("users");
            var json = JsonConvert.SerializeObject(user);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var a = await colRef.AddAsync(dictionary);
            return a;
        }

        public async void ChooseCanteen(string canteenName, long? pearId)
        {
            Query userQuery = firestoreDb.Collection("users");
            QuerySnapshot userSnapshots = await userQuery.GetSnapshotAsync();
            User currentUser = null;

            foreach (DocumentSnapshot documentSnapshot in userSnapshots.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Dictionary<string, object> user = documentSnapshot.ToDictionary();
                    string json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    User newUser = JsonConvert.DeserializeObject<User>(json);
                    newUser.UserId = documentSnapshot.Id;
                    if (newUser.Vkid == pearId)
                    {
                        currentUser = newUser;
                    }
                }
            }
            DocumentReference empRef;
            if (currentUser == null)
            {
                empRef = AddUser(pearId).Result;
                currentUser = new User() { Vkid = pearId, UserId = empRef.Id };
            }
            else
            {
                empRef = firestoreDb.Collection("users").Document(currentUser.UserId);
            }
            Query canteenQuery = firestoreDb.Collection("canteens");
            QuerySnapshot canteenSnapshots = await canteenQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in canteenSnapshots.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Dictionary<string, object> canteen = documentSnapshot.ToDictionary();
                    string json = JsonConvert.SerializeObject(canteen);
                    Canteen newCanteen = JsonConvert.DeserializeObject<Canteen>(json);
                    newCanteen.CanteenId = documentSnapshot.Id;
                    if (newCanteen.Name == canteenName)
                    {
                        currentUser.Canteen = newCanteen;
                    }
                }
            }
            await empRef.SetAsync(currentUser, SetOptions.Overwrite);
        }
    }
}