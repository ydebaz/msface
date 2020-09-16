using System;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace library
{
    class Program
    {
        const string SUBSCRIPTION_KEY = //enter you sub key here as a string 

        // replace <myresourcename> with the string found in your endpoint URL
        const string ENDPOINT =
            "https://<myresourcename>.cognitiveservices.azure.com/";
        static async Task Main(string[] args)
        {
            // Authenticate.
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            
            var aceattr = new List<FaceAttributeType?> { FaceAttributeType.Age, FaceAttributeType.Glasses };
           // string s = Console.ReadLine();
           // Console.WriteLine(s);
            try
            {
                // var faces = await client.Face.DetectWithUrlAsync(s,returnFaceAttributes:aceattr);
                
                string persongroupID = //replace here
                var kkk = await client.PersonGroup.GetAsync(persongroupID);
              //  Console.WriteLine(kkk.ToString());
                if (!kkk.Equals(null))
                {
                    Console.WriteLine("a  persongroup already exists ,do you want to use it or create another one ? reply with 'use' or 'new' ");
                    string qw = Console.ReadLine();
                    if (qw != "new" && qw != "use")
                    {
                        Console.WriteLine("not a valid answer");

                    }
                    else if (qw == "new")
                    {
                        await client.PersonGroup.DeleteAsync(persongroupID);
                        await client.PersonGroup.CreateAsync(persongroupID, //replave here with string);
                    }
                }
                else {
                    await client.PersonGroup.CreateAsync(persongroupID, //replave here with string);

                }
                Console.WriteLine("do you want to add new faces and people to the person group and retrain it or skip to identification ? reply 'add' or 'skip' ");
                string q = Console.ReadLine();
                if (q != "add" && q != "skip")
                {
                    Console.WriteLine("not a valid answer");

                }
                else if (q == "add")
                {
                    string k;
                    Console.WriteLine("please enter the name of each person and then a url to thies picture for training purposes");
                    Console.WriteLine("when finished enter '.'");
                    k = Console.ReadLine();

                    while (k != ".")
                    {
                        var friend1 = await client.PersonGroupPerson.CreateAsync(
        // Id of the PersonGroup that the person belonged to
        persongroupID,
        // Name of the person
        k);
                        string url = Console.ReadLine();
                        await client.PersonGroupPerson.AddFaceFromUrlAsync(persongroupID, friend1.PersonId, url);

                        k = Console.ReadLine();
                    }
                    Console.WriteLine("please wait until we finish training");

                    await client.PersonGroup.TrainAsync(persongroupID);

                    TrainingStatus trainingStatus = null;
                    while (true)
                    {
                        trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(persongroupID);

                        if (trainingStatus.Status != TrainingStatusType.Running)
                        {
                            break;
                        }

                        await Task.Delay(1000);
                    }
                    Console.WriteLine("training finished");
                }
                Console.WriteLine("if you are using the free tier you are limited to 20 calls per minute so please leave some time , thanks ");
                Console.WriteLine("please enter the number of images you want to identify and press enter ");
                int e =Convert.ToInt32( Console.ReadLine());
                Console.WriteLine(e);
                for (int i = 0; i < e; i++)
                {
                    Console.WriteLine("please enter a url to identify the faces then press enter ");
                    string urli = Console.ReadLine();
                    ///
                    var faces = await client.Face.DetectWithUrlAsync(urli);
                    List<Guid?> faceIds = new List<Guid?>();
                    foreach (var face in faces)
                    {
                        faceIds.Add(face.FaceId);
                        // Console.WriteLine(face.FaceId + " " + face.FaceAttributes.Age + " " + face.FaceAttributes.Glasses);
                    }
                    // var faceIds = faces.Select(face => face.FaceId.Value).ToList<Guid?>();

                    var results = await client.Face.IdentifyAsync(faceIds, persongroupID);
                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Count == 0)
                        {
                            Console.WriteLine("No one identified");
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await client.PersonGroupPerson.GetAsync(persongroupID, candidateId);
                            Console.WriteLine("Identified as {0}", person.Name);
                        }
                    }
                    ///   foreach (var face in faces) {
                    //    Console.WriteLine(face.FaceId + " " + face.FaceAttributes.Age + " " + face.FaceAttributes.Glasses);
                    // }
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.Message+" "+ex.ToString()); }
        }
        /*
 *	AUTHENTICATE
 *	Uses subscription key and region to create a client.
 */
        public static IFaceClient Authenticate(string endpoint, string key)
        {
           // return new FaceClient(FaceAttributes,)
           return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
    }
}
