﻿using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeploymentSerializer
{
    /// <summary>
    /// Class containing serialization methods for storing class data in and out of the engine.
    /// </summary>
    public static class DeploymentSerializer
    {
        private const string filenamePrefix = "DS_";
        private const string fileNameExtension = ".bytes";
        private const string fileNameTrackerName = "FNTP";

        private static string resourcesFilePath = UnityTools.getProjectFilePath() + "/Resources/";
        private static string nonPersistentFilePath = UnityTools.getProjectFilePath() + "/DevelopmentBinaries/";
        private static string fileNamesTrackerPath = UnityTools.getProjectFilePath() + "/Resources/FNTP.bytes";

        

        #region Saving Object Methods

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
                        " binary stored in the Resources Folder. Object saving process is being aborted!", SerializerLogType.Error);
                    return;
                }
                catch (Exception)
                {
                    DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to open the FileNameTracker" +
                        " binary stored in the Resources Folder. Onject saving process is being aborted!", SerializerLogType.Error);
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
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    "Resources Folder for persistance." +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
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
                    "Process is being aborted!", SerializerLogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the FileNameTracker object. " +
                    "Process is being aborted!", SerializerLogType.Error);
                return;
            }

            //Log the save if messages are enabled
            DS_MessageLogger.logMessageToUnityConsole(SystemTools.getObjectName(objectToSave) + " was saved successfully", SerializerLogType.Standard);
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
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    "DeveloperBinaries Folder. " +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
                return;
            }

            //Log the save if messages are enabled
            DS_MessageLogger.logMessageToUnityConsole(SystemTools.getObjectName(objectToSave) + " was saved successfully", SerializerLogType.Standard);
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
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
                return;
            }
            catch (Exception)
            {
                DS_MessageLogger.logMessageToUnityConsole("An unknown failure occured when trying to serialize the provided object to the " +
                    " Persistent File Path in build." +
                    "Process is being aborted! \n" +
                    "Class Name: " + SystemTools.getObjectName(objectToSave), SerializerLogType.Error);
                return;
            }

            //Log the save if messages are enabled
            DS_MessageLogger.logMessageToUnityConsole(SystemTools.getObjectName(objectToSave) + " was saved successfully", SerializerLogType.Standard);
        }

        #endregion

        #region Directory Checks and Creation

        /// <summary>
        /// Makes sure the Resources folder exists under the Assets folder. If it doesn't exist, create it.
        /// </summary>
        private static void makeSureResourcesExists ()
        {
            //If the folder doesn't exist, create it
            if (Directory.Exists(resourcesFilePath) == false)
            {
                Directory.CreateDirectory(resourcesFilePath);
                
                //Log message that we're creating a Folder for the devs
                DS_MessageLogger.logMessageToUnityConsole("Failed to find the Resources folder in your Assets folder. Creating Resources folder " +
                    "for your project. \n" +
                    "This is required for any persistent object saving between the engine and the game build.", SerializerLogType.Standard);                
            }
        }

        /// <summary>
        /// Makes sure the folder used for non-deplyed files is available to save binaries to.
        /// </summary>
        private static void makeSureNonPersistentPathExists ()
        {
            //If the folder doesn't exist, create it
            if (Directory.Exists(nonPersistentFilePath) == false)
            {
                Directory.CreateDirectory(nonPersistentFilePath);

                //Log message that we're creating a Folder for the devs
                DS_MessageLogger.logMessageToUnityConsole("Failed to find the DeveloperBinaries folder in your Assets folder. " +
                    "Creating the folder for your project. \n" +
                    "This folder is used by the asset to store saves that will not be accessible in builds of the game", SerializerLogType.Standard);
            }
        }

        #endregion

        #region Load Object Methods

        /// <summary>
        /// Call this in the game build to take all persistent files and load them into the PersistentPath directory
        /// </summary>
        /// <returns>Boolean indicating if the unpack was sucessful</returns>
        public static bool unpackPersistentSaves ()
        {





            //Make sure the file exists
            TextAsset ta = Resources.Load(fileNameTrackerName) as TextAsset;
            if (ta == null){
                DS_MessageLogger.logMessageToUnityConsole("Failed to find the " + fileNameTrackerName + ".bytes file, did you save any files to" +
                    "initize it?", SerializerLogType.Warning);
                return false;
            }

            //Deserialize the file to read from
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream mStream = new MemoryStream(ta.bytes);
            FileNameTracker fileNameTracker = binaryFormatter.Deserialize(mStream) as FileNameTracker;
            
            //Load all saved files into the persistent folder
            foreach (string fileName in fileNameTracker.fileNames)
            {

            }


            return true;
        }

        #endregion
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
    /// Object used to store developer settings for logging states
    /// </summary>
    [System.Serializable]
    internal class LoggingSettingsTracker
    {
        internal bool displayEventMessages;
        internal bool logBuildMessages;
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
        /// <param name="obj">General object to grab name of</param>
        /// <returns>Class Name of Object</returns>
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
        private static string buildLogFilePath = UnityTools.getPersistentFilePath() + "/DeploymentSerializerLog.txt";

        private static bool displayEventMessages = true;
        private static bool logMessagesInBuild = true;

        #region Set Logging States

        /// <summary>
        /// Allows the Deployment Serializer to write log messages to the Unity Console
        /// </summary>
        public static void enableEventMessages() { displayEventMessages = true; }
        /// <summary>
        /// Prevents the Deployment Serializer to write log messages to the Unity Console
        /// </summary>
        public static void disableEventMessages() { displayEventMessages = false; }

        /// <summary>
        /// Allows the Deployment Serializer to write logs to the log file in game build
        /// </summary>
        public static void enableBuildLogs() { logMessagesInBuild = true; }
        /// <summary>
        /// Prevents the Deployment Serializer to write logs to the log file in game build
        /// </summary>
        public static void disableBuildLogs() { logMessagesInBuild = false; }

        #endregion

        /// <summary>
        /// Standard log method for submitting general log messages that are formatted before displaying.
        /// </summary>
        /// <param name="message">Message to embed into log report</param>
        internal static void logMessageToUnityConsole (string message, SerializerLogType logType)
        {
            if (displayEventMessages == false) return;

            string output = "Deployment Serializer Log Message: \n" +
                "---------------------------------------------- \n" +
                message + "\n" +
                "----------------------------------------------";

            Debug.Log(output);
        }

        /// <summary>
        /// Called when the Deployment Serializer was asked to save an object to a binary file but the object is not tagged as serializable.
        /// </summary>
        internal static void logNonSerializableObjectCall (object t)
        {
            string message = "Deployment Serializer was called to saveObject() on an Object that was not tagged as serializable by the programmer " +
                "and cannot be saved to file. Please include the [System.Serializable] attribute on the object. \n" +
                "Class Name: " + SystemTools.getObjectName(t);
            logMessageToUnityConsole(message, SerializerLogType.Warning);
        }

        /// <summary>
        /// Appends a log message to the log file 
        /// </summary>
        /// <param name="message">Contents of message to be formatted for the log</param>
        /// <param name="logType">Message type for log to be catagorized by</param>
        internal static void logMessageToBuildFile (string message, SerializerLogType logType)
        {
            if (logMessagesInBuild == false) return;


            string output = "";

            //Add correct header for log type
            switch (logType)
            {
                case SerializerLogType.Standard:
                    output += "Standard Log Message:\n";
                    break;
                case SerializerLogType.Warning:
                    output += "Warning!\n";
                    break;
                case SerializerLogType.Error:
                    output += "ERROR!\n";
                    break;
            }

            //Format message
            output += "---------------------------------------------- \n"
                + message + "\n" +
                "---------------------------------------------- \n" +
                "~ \n";

            //Nest file modification in try/catch to prevent IO based crashing
            try
            {
                //Append the new log to the file, creates file if it doesn't exist
                File.AppendAllText(buildLogFilePath, output);
            } 
            catch (IOException)
            {
                logMessageToUnityConsole("Deployment Serializer failed to append to the log file!", SerializerLogType.Error);
            }
        }

        /// <summary>
        /// Clears the build log file of all entries, effectively refreshes file
        /// </summary>
        public static void clearBuildLog ()
        {
            //Only clear if file exists
            if (File.Exists(buildLogFilePath))
            {
                File.WriteAllText(buildLogFilePath, "");
            } 
        }

        /// <summary>
        /// Parsed the build log into an array of strings where each element is a single log entry. Single empty string if log is emtpy.
        /// </summary>
        /// <returns>Array of log entries</returns>
        public static string[] parseBuildLogsToArray ()
        {
            if (File.Exists(buildLogFilePath) == false) return new string[] { "" };

            string contents = File.ReadAllText(buildLogFilePath);
            string[] parsedMessages = Regex.Split(contents, "~");

            Array.Resize(ref parsedMessages, parsedMessages.Length - 1);

            for (int i = 0; i < parsedMessages.Length; i++)
            {
                if (parsedMessages[i].Substring(0,2) == "\n")
                {
                    parsedMessages[i] = parsedMessages[i].Substring(2);
                }
            }

            return parsedMessages;
        }
    }

    /// <summary>
    /// Enum for tracking which log level the MessageLogger should log messages to in the Unity Console
    /// </summary>
    internal enum SerializerLogType { Standard, Warning, Error }
}
