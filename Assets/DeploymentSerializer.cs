using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace DeploymentSerializer
{
    /// <summary>
    /// Class containing serialization methods for storing class data in and out of the engine.
    /// </summary>
    public static class DeploymentSerializer
    {
        private const string filenamePrefix = "DS_";
        private const string fileNameExtension = ".bytes";

        private static string resourcesFilePath = UnityTools.getProjectFilePath() + "/Resources/";
        private static string nonPersistentFilePath = UnityTools.getProjectFilePath() + "/DevelopmentBinaries/";
        private static string fileNamesTrackerPath = UnityTools.getProjectFilePath() + "/Resources/FNTP.bytes";

        /// <summary>
        /// Saves the provided Object to a binary file. If marked as persistent the file is saved to the Resources folder to be rebuilt for 
        /// builds of the game. If called in game build the isPersistentInBuild parameter is ignored.
        /// </summary>
        /// <typeparam name="T">Type of Object to save</typeparam>
        /// <param name="objectToSave">Object to be saved</param>
        /// <param name="isPersistentInBuild">Determines if the file is saved to the resources folder to keep persistent in game build</param>
        public static void saveObject (object objectToSave, string fileName, bool isPersistentInBuild)
        {
            //Preliminary check to make sure the provided Object is serializable
            if (SystemTools.isSerializable(objectToSave) == false)
            {
                DS_MessageLogger.logNonSerializableObjectCall(objectToSave);
                return;
            }

            //Check if we're running in build or engine - Save to correct location
            if (UnityTools.isRunningInEngine())
            {
                //Guarentee file paths exists
                makeSureResourcesExists();
                makeSureNonPersistentPathExists();

                //Save to resources for persistence
                if (isPersistentInBuild)
                {
                    serializeObject_Persistent(objectToSave, fileName);
                }
                //Save to DevelopmentBinaries for dev only
                else
                {
                    serializeObject_Developer(objectToSave, fileName);
                }                
            }
            //Otherwise we're in a build of the game and can save normally
            else
            {
                serializeObject_Build(objectToSave, fileName);
            }
        }

        /// <summary>
        /// Saves the object to a binary file in the Resources project folder
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="objectToSave">Object to be serialized</param>
        /// <param name="fileName">Name to save file under</param>
        private static void serializeObject_Persistent (object objectToSave, string fileName)
        {
            //Initialize objects for use
            BinaryFormatter binaryFormatter;
            FileStream fileStream;
            FileNameTracker fileNameTracker;

            ////Get a valid copy of the FileNameTracker
            //If the file tracker exists we deserialize it
            if (SystemTools.fileExists(fileNamesTrackerPath))
            {
                try
                {
                    binaryFormatter = new BinaryFormatter();
                    fileStream = new FileStream(fileNamesTrackerPath, FileMode.Open);
                    fileNameTracker = binaryFormatter.Deserialize(fileStream) as FileNameTracker;
                    fileStream.Close();
                }
                catch (IOException)
                {
                    DS_MessageLogger.logMessageToUnityConsole("An unexpected IO failure occured when trying to open the FileNameTracker" +
                        " binary stored in the Resources Folder. Object saving process is being aborted!", LogType.Error);
                    return;
                }
                catch (Exception)
                {
                    DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to open the FileNameTracker" +
                        " binary stored in the Resources Folder. Onject saving process is being aborted!", LogType.Error);
                    return;
                }
            }
            //Otherwise we need to make it
            else
            {
                fileNameTracker = new FileNameTracker();
            }

            //Build file name and save reference
            fileName = filenamePrefix + fileName;
            string filePath = resourcesFilePath + fileName + fileNameExtension;

            //Serialize the new Object provided by the Dev
            try
            {                
                //Create stream and formatter
                binaryFormatter = new BinaryFormatter();
                fileStream = new FileStream(filePath, FileMode.Create);
                //Serialize file and close stream
                binaryFormatter.Serialize(fileStream, objectToSave);
                fileStream.Close();
            }
            catch (IOException)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unexpected IO failure occured when trying to serialize the provided object to the " +
                    "Resources Folder for persistance. " +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    "Resources Folder for persistance." +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }

            //Serialize the FileNameTracker for persistence
            try
            {                
                //Add the new file name
                fileNameTracker.fileNames.Add(fileName);
                //Create stream and formatter
                binaryFormatter = new BinaryFormatter();
                fileStream = new FileStream(fileNamesTrackerPath, FileMode.Create);
                //Serialize the file and close the stream
                binaryFormatter.Serialize(fileStream, fileNameTracker);
            }
            catch (IOException)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unexpected IO failure occured when trying to serialize the FileNameTracker object. " +
                    "Process is being aborted!", LogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the FileNameTracker object. " +
                    "Process is being aborted!", LogType.Error);
                return;
            }            
        }

        /// <summary>
        /// Saves the object to a binary file in the DeveloperBinaries folder
        /// </summary>
        /// <typeparam name="T">Object type to be serialized</typeparam>
        /// <param name="objectToSave">Object to serialize</param>
        /// <param name="fileName">Name to save the file under</param>
        private static void serializeObject_Developer (object objectToSave, string fileName)
        {
            //Initialize objects for use
            BinaryFormatter binaryFormatter;
            FileStream fileStream;

            //Build file name and save reference
            fileName = filenamePrefix + fileName;
            string filePath = nonPersistentFilePath + fileName + fileNameExtension;

            //Serialize the new Object provided by the Dev
            try
            {
                //Create stream and formatter
                binaryFormatter = new BinaryFormatter();
                fileStream = new FileStream(filePath, FileMode.Create);
                //Serialize file and close stream
                binaryFormatter.Serialize(fileStream, objectToSave);
                fileStream.Close();
            }
            catch (IOException)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unexpected IO failure occured when trying to serialize the provided object to the" +
                    "DeveloperBinaries folder. " +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    "DeveloperBinaries Folder. " +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }
        } 

        /// <summary>
        /// Saves the object to a binary file in the PersistentPath for post deployment saves
        /// </summary>
        /// <typeparam name="T">Object type to be serialized</typeparam>
        /// <param name="objectToSave">Object to be serialized</param>
        /// <param name="fileName">Name to save the file under</param>
        private static void serializeObject_Build (object objectToSave, string fileName)
        {
            //Initialize objects for use
            BinaryFormatter binaryFormatter;
            FileStream fileStream;

            //Build file name and save reference
            fileName = filenamePrefix + fileName;
            string filePath = UnityTools.getPersistentFilePath() + "/" + fileName + fileNameExtension;

            //Serialize the new Object provided by the Dev
            try
            {
                //Create stream and formatter
                binaryFormatter = new BinaryFormatter();
                fileStream = new FileStream(filePath, FileMode.Create);
                //Serialize file and close stream
                binaryFormatter.Serialize(fileStream, objectToSave);
                fileStream.Close();
            }
            catch (IOException)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unexpected IO failure occured when trying to serialize the provided object to the" +
                    " Persistent File Path in build. " +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    " Persistent File Path in build." +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), LogType.Error);
                return;
            }
        }

        /// <summary>
        /// Makes sure the Resources folder exists under the Assets folder. If it doesn't exist, create it.
        /// </summary>
        private static void makeSureResourcesExists ()
        {
            //If the folder doesn't exist, create it
            if (Directory.Exists(resourcesFilePath) == false)
            {
                Directory.CreateDirectory(resourcesFilePath);
                //TODO - Proper message
                DS_MessageLogger.logMessageToUnityConsole("No resources folder, creating one", LogType.Standard);
            }
        }

        /// <summary>
        /// Makes sure the folder used for non-deplyed files is available to save binaries to.
        /// </summary>
        private static void makeSureNonPersistentPathExists ()
        {
            if (Directory.Exists(nonPersistentFilePath) == false)
            {
                Directory.CreateDirectory(nonPersistentFilePath);
                //TODO - Proper Message
                DS_MessageLogger.logMessageToUnityConsole("No non-persistent path, creating one", LogType.Standard);
            }
        }        
    }

    /// <summary>
    /// Class dedicated to reading and saving all persistent file names for when unpacking binaries post build
    /// </summary>
    [System.Serializable]
    internal class FileNameTracker
    {
        internal List<string> fileNames;

        /// <summary>
        /// Constructor
        /// </summary>
        internal FileNameTracker ()
        {
            fileNames = new List<string>();
        }
    }

    /// <summary>
    /// Class to hold self contained support methods used by other members of the namespace.
    /// </summary>
    internal static class SystemTools
    {
        /// <summary>
        /// Returns if the provided object is serializable
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="objectToCheck">Object to check for serializable attribute</param>
        /// <returns>If the Object is serializable</returns>
        internal static bool isSerializable (object objectToCheck)
        {
            Type type = objectToCheck.GetType();
            return type.IsSerializable;
        }

        /// <summary>
        /// Returns if the file at the given path exists
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>If file exists</returns>
        internal static bool fileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Getter method returns the name of the class/object as viewed in the editor
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static string getObjectName (object obj)
        {
            return obj.GetType().Name;
        }
    }

    /// <summary>
    /// Additional tools to help with extra functionality newer programmers may find useful. 
    /// </summary>
    public static class UnityTools
    {
        /// <summary>
        /// Is the game currently running in the Unity Engine?
        /// </summary>
        /// <returns>True if in engine, false if in build</returns>
        public static bool isRunningInEngine ()
        {
            return UnityEngine.Application.isEditor;
        }

        /// <summary>
        /// Returns the file path of the machine leading to the folder holding the game files. 
        /// When in engine -> Returns the 'projectFolder/Assets'
        /// When in build -> Returns the build folder
        /// </summary>
        /// <returns>String - File path leading to project folder</returns>
        public static string getProjectFilePath ()
        {
            return Application.dataPath;
        }

        /// <summary>
        /// Returns the file path to a persistent folder on the end machine typically used
        /// for saving and modifying data in builds of the game.
        /// </summary>
        /// <returns>String - persistent folder for saving/modifying data in builds</returns>
        public static string getPersistentFilePath ()
        {
            return Application.persistentDataPath;
        }
    }

    /// <summary>
    /// Class handles all logging related functions for when the asset needs to log a status message 
    /// with developers using the asset. 
    /// </summary>
    internal static class DS_MessageLogger
    {
        /// <summary>
        /// Standard log method for submitting general log messages that are formatted before displaying.
        /// </summary>
        /// <param name="message">Message to embed into log report</param>
        internal static void logMessageToUnityConsole (string message, LogType logType)
        {
            string output = "Deployment Serializer Log Message: \n" +
                "---------------------------------------------- \n" +
                message + "\n" +
                "----------------------------------------------";

            UnityEngine.Debug.Log(output);
        }

        /// <summary>
        /// Called when the Deployment Serializer was asked to save an object to a binary file but the object is not tagged as serializable.
        /// </summary>
        internal static void logNonSerializableObjectCall <T> (T t)
        {
            string message = "Deployment Serializer was called to saveObject() on an Object that was not tagged as serializable by the programmer " +
                "and cannot be saved to file. Please include the [System.Serializable] attribute on the object. \n" +
                "Class Name: " + t.GetType().Name;
            logMessageToUnityConsole(message, LogType.Warning);
        }
    }

    /// <summary>
    /// Enum for tracking which log level the MessageLogger should log messages to in the Unity Console
    /// </summary>
    internal enum LogType { Standard, Warning, Error }
}
