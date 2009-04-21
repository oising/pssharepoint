#region license
// Copyright 2008 HubKey, LLC. (http://www.hubkey.com)
// 
// This software is provided under the Creative Commons Attribution-Noncommercial-Share Alike 3.0 license.
// For the full licence see http://creativecommons.org/licenses/by-nc-sa/3.0

//  You are free:
//    * to Share — to copy, distribute, display, and perform the work
//    * to Remix — to make derivative works
//  Under the following conditions:
//    * Attribution. You must attribute the work in the manner specified by the author or licensor (but not in any way that suggests that they endorse you or your use of the work).
//    * Noncommercial. You may not use this work for commercial purposes.
//    * Share Alike. If you alter, transform, or build upon this work, you may distribute the resulting work only under the same or similar license to this one.
//    * Any of the above conditions can be waived if you get permission from the copyright holder.
//    * Apart from the remix rights granted under this license, nothing in this license impairs or restricts the author's moral rights.
// 
// The above copyright notice and permission notice shall be included in all copies or derivatives of this software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace HubKey.Net.FrontPageRPC
{
    public enum ListTemplateType
    {
        Folder = -100,
        Unknown = -1,
        DocumentLibrary = 65,
        Survey = 66,
        Links = 67,
        Announcements = 68,
        Contacts = 69,
        UserInformation = 70,
        WebPartCatalog = 71,
        ListTemplateCatalog = 72,
        XMLForm = 73,
        MasterPageCatalog = 74,
        NoCodeWorkflows = 75,
        WorkflowProcess = 76,
        WebPageLibrary = 77,
        GenericList = 100,
        SmartFolder = 101,
        Events = 106,
        Tasks = 107,
        DiscussionBoard = 108,
        PictureLibrary = 109,
        DataSources = 110,
        WebTemplateCatalog = 111,
        CustomGrid = 120,
        DataConnectionLibrary = 130,
        WorkflowHistory = 140,
        GanttTasks = 150,
        Meetings = 200,
        Agenda = 201,
        MeetingUser = 202,
        Decision = 204,
        MeetingObjective = 207,
        TextBox = 210,
        ThingsToBring = 211,
        HomePageLibrary = 212,
        Posts = 301,
        Comments = 302,
        Categories = 303,
        IssueTracking = 1100,
        AdminTasks = 1200
    }

    [Flags]
    public enum RenameOptionEnum : int
    {
        /// <summary>
        /// Creates the parent directory if it does not already exist. This flag is analogous to the "createdir" PUT-OPTION-VAL (as specified in Put-Option) and has the same semantics.
        /// </summary>
        CreateDir = 1,
        /// <summary>
        /// Requests that servers, implementing link fixup, fix the linked files other than those moved. The server MAY ignore this flag.
        /// </summary>
        FindBacklinks = 2,
        /// <summary>
        /// Do not perform link fixup on links in moved documents. This parameter is used in publishing scenarios. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send this option. The server MAY ignore this.
        /// </summary>
        NoChangeAll = 4,
        /// <summary>
        /// Simulates the move of a directory rather than a file. Clients conforming to the FrontPage Server Extensions:Website Management MUST NOT send this option; the server SHOULD ignore this flag for the usage defined in this document.
        /// </summary>
        PatchPrefix = 8,
        /// <summary>
        /// The client MUST send "none" if it does not want to specify any of the options .
        /// </summary>
        None = 16
    }

    [Flags]
    public enum PutOptionEnum : int
    {
        /// <summary>
        /// Uses the date and time the document was last modified, as specified in the inbound metainfo, rather than the extent of time on the server.
        /// </summary>
        Overwrite = 1, 
        /// <summary>
        /// The parent directory is created if it does not exist. When this option is not sent by the client, the server MUST require that the parent directory of a file or folder exists; if the client sends this option, the server SHOULD create the immediate parent of the file being created if needed and if possible. For example, if this option is sent and folder1/folder2/file.txt is being created, the server SHOULD create folder2 if needed, but SHOULD NOT create folder1 if it does not already exist.
        /// </summary>
        CreateDir = 2, 
        /// <summary>
        /// Not used by the FrontPage Server Extensions Remote Protocol. Preserves information about who created the file and when. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send this option. The server MAY ignore this option. If the server wants to honor this option, it SHOULD do additional authorization and ignore the option if the authorization fails.
        /// </summary>
        MigrationSemantics = 4, 
        /// <summary>
        /// If this flag is specified, the server does all the needed checking to ensure that all the files can be updated before changing the first one. The server MAY ignore this.
        /// </summary>
        Atomic = 8, 
        /// <summary>
        /// The document is checked in after it is saved. This flag is only used to support long-term checkout operations. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send this. Servers MAY ignore this parameter if they choose not to support long-term checkout.
        /// </summary>
        Checkin = 16,
        /// <summary>
        /// Valid only if checkin is specified. Notifies the source control of the new content (checkin), but keeps the document checked out. (This is the equivalent to checking the document in, and then checking it out again.) Clients conforming to the FrontPage Server Extensions Remote Protocol defined in this document MUST NOT send this option. Servers MAY ignore this parameter.
        /// </summary>
        Checkout = 32,
        /// <summary>
        /// Uses the date and time the document was last modified to determine whether the item has been concurrently modified by another user. This flag is used to prevent race conditions where two users could edit the same data. If this flag is specified and the inbound modification time does not match the value on the server, the server MUST reject the upload. The client SHOULD send this flag unless a higher level has indicated it needs to overwrite changes.
        /// </summary>
        Edit = 64,
        /// <summary>
        /// Not used by the FrontPage Server Extensions Remote Protocol. Acts as though versioning is enabled, even if it is not. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send this option. Servers MAY ignore this parameter.
        /// </summary>
        ForceVersions = 128,
        /// <summary>
        /// Not used by the FrontPage Server Extensions Remote Protocol. Acts as though versioning is enabled, even if it is not. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send this option. Servers MAY ignore this parameter.
        /// </summary>
        ListThickets = 256,
        /// <summary>
        /// Specifies that the associated file is a thicket supporting file. The server SHOULD detect that the upload includes a thicket supporting file and infer this flag.
        /// </summary>
        Thicket = 512,
        /// <summary>
        /// Default put option - Overwrite | CreateDir
        /// </summary>
        Default = Overwrite | CreateDir
    }

    public enum ErrorFlagsEnum
    {
        /// <summary>
        /// Associated with an error that can be ignored.
        /// </summary>
        KeepGoing,
        /// <summary>
        /// Processing stops when an error occurs.
        /// </summary>
        StopOnFirst,
        /// <summary>
        /// Not currently implemented.
        /// </summary>
        Atomic 
    }

    public enum MetaTypeEnum
    {
        Boolean = 66, Double = 68, Empty = 69, FileSystemTime = 70, Integer = 73, LongText = 76, String = 83, Time = 84, IntegerVector = 85, StringVector = 86
    }

    public enum MetaAccessEnum
    {
        ReadOnly = 82, ReadWrite = 87, NoAccess = 88
    }

    public enum GetOptionEnum
    {
        /// <summary>
        /// Do not check out the file.
        /// </summary>
        none = 1, 
        /// <summary>
        /// Check out the file exclusively, which fails if the file is already checked out by another user.
        /// </summary>
        chkoutExclusive = 2, 
        /// <summary>
        /// Check out the file nonexclusively, if the source control system in use is configured to allow nonexclusive check-outs.
        /// </summary>
        chkoutNonExclusive = 3
    }

    public enum DepthEnum
    {
        /// <summary>
        /// The method is applied only to the resource.
        /// </summary>
        Zero,
        /// <summary>
        /// The method is applied to the resource and to its immediate children.
        /// </summary>
        One,
        /// <summary>
        /// The method is applied only to resources immediately subordinate to the target Uniform Resource Identifier (URI) but the target resource itself is excluded. 
        /// </summary>
        OneNoRoot,
        /// <summary>
        /// The method is applied to the resource and to all of its children.
        /// </summary>
        Infinity,
        /// <summary>
        /// The method is applied recursively to all resources subordinate to the target but the target resource itself is excluded. 
        /// </summary>
        InfinityNoRoot
    }

    public enum Direction
    {
        Upload, Download
    }

    public enum EscapeOption
    {
        Escape, Unescape, None
    }

    public enum ProcessingType
    {
        Syncronous, Asyncronous
    }
}
