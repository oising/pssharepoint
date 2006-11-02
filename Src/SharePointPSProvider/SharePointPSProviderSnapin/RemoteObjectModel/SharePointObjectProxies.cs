#region BSD License Header
/*
 * Copyright (c) 2006, Oisin Grehan @ Nivot Inc (www.nivot.org)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
 * Neither the name of Nivot Incorporated nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

// TODO: implement this (!) (was previously a test object model)

/*
namespace Microsoft.SharePoint {

	public class SPObject {
		public string Title;

		protected SPObject(string title) {
			Title = title;
		}
	}

	public class SPSite : SPObject {
		SPWeb m_rootWeb;

		public SPSite(string title)
			: base(title) {

			m_rootWeb = new SPWeb(String.Empty); // root
			m_rootWeb.Alerts.Add(new SPAlert("alert1"));
			m_rootWeb.Lists.Add(new SPList("list1"));

			foreach (string roleName in (new string[] { "reader", "contributor", "web designer", "administrator" })) {
				m_rootWeb.Roles.Add(new SPRole(roleName));
			}

			for (int i = 0 ; i < 5 ; i++) {
				m_rootWeb.Webs.Add(new SPWeb("web" + i));
			}

			m_rootWeb.Webs["web4\\"].Webs.Add(new SPWeb("web4_0")); // intellisense loves this one...
		}

		public SPWeb RootWeb {
			get {
				return m_rootWeb;
			}
		}
	}

	public class TitleIndexedCollection<T> : Collection<SPObject> where T : SPObject {
		public T this[string title] {
			get {
				foreach (T item in this) {
					if (item.Title == title) {
						return item;
					}
				}
				return null;
			}
		}
	}

	public class SPWeb : SPObject {
		SPWebCollection m_webs = new SPWebCollection();
		SPAlertCollection m_alerts = new SPAlertCollection();
		SPUserCollection m_users = new SPUserCollection();
		SPRoleCollection m_roles = new SPRoleCollection();
		SPListCollection m_lists = new SPListCollection();
		SPGroupCollection m_groups = new SPGroupCollection();

		public SPWeb(string title)
			: base(title + SharePointProvider.PathSeparator) { // terminate path with separator
		}

		#region Alerts, Groups, Lists, Roles, Users, Webs
		public SPWebCollection Webs {
			get {
				return m_webs;
			}
		}

		public SPAlertCollection Alerts {
			get {
				return m_alerts;
			}
		}

		public SPGroupCollection Groups {
			get {
				return m_groups;
			}
		}

		public SPListCollection Lists {
			get {
				return m_lists;
			}
		}

		public SPRoleCollection Roles {
			get {
				return m_roles;
			}
		}

		public SPUserCollection Users {
			get {
				return m_users;
			}
		}
		#endregion
	}

	public class SPWebCollection : TitleIndexedCollection<SPWeb> {
		public SPWebCollection() {
		}
	}

	public class SPAlert : SPObject {
		public SPAlert(string title)
			: base(title) {
		}
	}

	public class SPAlertCollection : TitleIndexedCollection<SPAlert> {
		public SPAlertCollection() {
		}
	}

	public class SPGroup : SPObject {
		private SPUserCollection m_users;

		public SPGroup(string title)
			: base(title) {

			m_users = new SPUserCollection();
		}

		public SPUserCollection Users {
			get { return m_users; }
		}
	}

	public class SPGroupCollection : TitleIndexedCollection<SPGroup> {
		public SPGroupCollection() {
		}
	}

	public class SPList : SPObject {
		public SPList(string title)
			: base(title) {
		}
	}

	public class SPListCollection : TitleIndexedCollection<SPList> {
		public SPListCollection() {
		}
	}

	public class SPRole : SPObject {
		private SPUserCollection m_users;

		public SPRole(string title)
			: base(title) {
			m_users = new SPUserCollection();
		}

		public SPUserCollection Users {
			get { return m_users; }
		}
	}

	public class SPRoleCollection : TitleIndexedCollection<SPRole> {
		public SPRoleCollection() {
		}
	}

	public class SPUser : SPObject {
		public SPUser(string title)
			: base(title) {
		}
	}

	public class SPUserCollection : TitleIndexedCollection<SPUser> {
		public SPUserCollection() {
		}
	}
}
*/