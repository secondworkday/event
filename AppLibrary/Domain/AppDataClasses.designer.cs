﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace App.Library
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	public partial class AppDC : MS.WebUtility.WebUtilityDC
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertExtendedTenantGroup(ExtendedTenantGroup instance);
    partial void UpdateExtendedTenantGroup(ExtendedTenantGroup instance);
    partial void DeleteExtendedTenantGroup(ExtendedTenantGroup instance);
    partial void InsertExtendedUser(ExtendedUser instance);
    partial void UpdateExtendedUser(ExtendedUser instance);
    partial void DeleteExtendedUser(ExtendedUser instance);
    partial void InsertParticipantGroup(ParticipantGroup instance);
    partial void UpdateParticipantGroup(ParticipantGroup instance);
    partial void DeleteParticipantGroup(ParticipantGroup instance);
    partial void InsertParticipant(Participant instance);
    partial void UpdateParticipant(Participant instance);
    partial void DeleteParticipant(Participant instance);
    #endregion
		
		public AppDC(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AppDC(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AppDC(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AppDC(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ExtendedTenantGroup> ExtendedTenantGroups
		{
			get
			{
				return this.GetTable<ExtendedTenantGroup>();
			}
		}
		
		public System.Data.Linq.Table<ExtendedUser> ExtendedUsers
		{
			get
			{
				return this.GetTable<ExtendedUser>();
			}
		}
		
		public System.Data.Linq.Table<ParticipantGroup> ParticipantGroups
		{
			get
			{
				return this.GetTable<ParticipantGroup>();
			}
		}
		
		public System.Data.Linq.Table<Participant> Participants
		{
			get
			{
				return this.GetTable<Participant>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ExtendedTenantGroup")]
	public partial class ExtendedTenantGroup : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _Address1;
		
		private string _Address2;
		
		private string _City;
		
		private string _State;
		
		private string _Zip;
		
		private string _Phone;
		
		private string _Website;
		
		private int _NumberOfSeatsContracted;
		
		private System.Nullable<int> _ConcurrentSeatsContracted;
		
		private string _ClientSettings;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnAddress1Changing(string value);
    partial void OnAddress1Changed();
    partial void OnAddress2Changing(string value);
    partial void OnAddress2Changed();
    partial void OnCityChanging(string value);
    partial void OnCityChanged();
    partial void OnStateChanging(string value);
    partial void OnStateChanged();
    partial void OnZipChanging(string value);
    partial void OnZipChanged();
    partial void OnPhoneChanging(string value);
    partial void OnPhoneChanged();
    partial void OnWebsiteChanging(string value);
    partial void OnWebsiteChanged();
    partial void OnNumberOfSeatsContractedChanging(int value);
    partial void OnNumberOfSeatsContractedChanged();
    partial void OnConcurrentSeatsContractedChanging(System.Nullable<int> value);
    partial void OnConcurrentSeatsContractedChanged();
    partial void OnClientSettingsChanging(string value);
    partial void OnClientSettingsChanged();
    #endregion
		
		public ExtendedTenantGroup()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Address1", DbType="nvarchar(100)")]
		public string Address1
		{
			get
			{
				return this._Address1;
			}
			set
			{
				if ((this._Address1 != value))
				{
					this.OnAddress1Changing(value);
					this.SendPropertyChanging();
					this._Address1 = value;
					this.SendPropertyChanged("Address1");
					this.OnAddress1Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Address2", DbType="nvarchar(100)")]
		public string Address2
		{
			get
			{
				return this._Address2;
			}
			set
			{
				if ((this._Address2 != value))
				{
					this.OnAddress2Changing(value);
					this.SendPropertyChanging();
					this._Address2 = value;
					this.SendPropertyChanged("Address2");
					this.OnAddress2Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_City", DbType="nvarchar(100)")]
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				if ((this._City != value))
				{
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._City = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_State", DbType="nvarchar(10)")]
		public string State
		{
			get
			{
				return this._State;
			}
			set
			{
				if ((this._State != value))
				{
					this.OnStateChanging(value);
					this.SendPropertyChanging();
					this._State = value;
					this.SendPropertyChanged("State");
					this.OnStateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Zip", DbType="nvarchar(10)")]
		public string Zip
		{
			get
			{
				return this._Zip;
			}
			set
			{
				if ((this._Zip != value))
				{
					this.OnZipChanging(value);
					this.SendPropertyChanging();
					this._Zip = value;
					this.SendPropertyChanged("Zip");
					this.OnZipChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Phone", DbType="nvarchar(20)")]
		public string Phone
		{
			get
			{
				return this._Phone;
			}
			set
			{
				if ((this._Phone != value))
				{
					this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._Phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Website", DbType="nvarchar(100)")]
		public string Website
		{
			get
			{
				return this._Website;
			}
			set
			{
				if ((this._Website != value))
				{
					this.OnWebsiteChanging(value);
					this.SendPropertyChanging();
					this._Website = value;
					this.SendPropertyChanged("Website");
					this.OnWebsiteChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NumberOfSeatsContracted")]
		public int NumberOfSeatsContracted
		{
			get
			{
				return this._NumberOfSeatsContracted;
			}
			set
			{
				if ((this._NumberOfSeatsContracted != value))
				{
					this.OnNumberOfSeatsContractedChanging(value);
					this.SendPropertyChanging();
					this._NumberOfSeatsContracted = value;
					this.SendPropertyChanged("NumberOfSeatsContracted");
					this.OnNumberOfSeatsContractedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ConcurrentSeatsContracted")]
		public System.Nullable<int> ConcurrentSeatsContracted
		{
			get
			{
				return this._ConcurrentSeatsContracted;
			}
			set
			{
				if ((this._ConcurrentSeatsContracted != value))
				{
					this.OnConcurrentSeatsContractedChanging(value);
					this.SendPropertyChanging();
					this._ConcurrentSeatsContracted = value;
					this.SendPropertyChanged("ConcurrentSeatsContracted");
					this.OnConcurrentSeatsContractedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ClientSettings", DbType="nvarchar(MAX)")]
		public string ClientSettings
		{
			get
			{
				return this._ClientSettings;
			}
			set
			{
				if ((this._ClientSettings != value))
				{
					this.OnClientSettingsChanging(value);
					this.SendPropertyChanging();
					this._ClientSettings = value;
					this.SendPropertyChanged("ClientSettings");
					this.OnClientSettingsChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ExtendedUser")]
	public partial class ExtendedUser : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private string _PreferredState;
		
		private string _PreferredLaborMarketArea;
		
		private string _Title;
		
		private string _Organization;
		
		private string _Address1;
		
		private string _Address2;
		
		private string _City;
		
		private string _State;
		
		private string _Zip;
		
		private string _Phone;
		
		private string _PhoneExtension;
		
		private string _SecureProperties;
		
		private string _CustomProperties;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnPreferredStateChanging(string value);
    partial void OnPreferredStateChanged();
    partial void OnPreferredLaborMarketAreaChanging(string value);
    partial void OnPreferredLaborMarketAreaChanged();
    partial void OnTitleChanging(string value);
    partial void OnTitleChanged();
    partial void OnOrganizationChanging(string value);
    partial void OnOrganizationChanged();
    partial void OnAddress1Changing(string value);
    partial void OnAddress1Changed();
    partial void OnAddress2Changing(string value);
    partial void OnAddress2Changed();
    partial void OnCityChanging(string value);
    partial void OnCityChanged();
    partial void OnStateChanging(string value);
    partial void OnStateChanged();
    partial void OnZipChanging(string value);
    partial void OnZipChanged();
    partial void OnPhoneChanging(string value);
    partial void OnPhoneChanged();
    partial void OnPhoneExtensionChanging(string value);
    partial void OnPhoneExtensionChanged();
    partial void OnSecurePropertiesChanging(string value);
    partial void OnSecurePropertiesChanged();
    partial void OnCustomPropertiesChanging(string value);
    partial void OnCustomPropertiesChanged();
    #endregion
		
		public ExtendedUser()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PreferredState", DbType="nvarchar(10)")]
		public string PreferredState
		{
			get
			{
				return this._PreferredState;
			}
			set
			{
				if ((this._PreferredState != value))
				{
					this.OnPreferredStateChanging(value);
					this.SendPropertyChanging();
					this._PreferredState = value;
					this.SendPropertyChanged("PreferredState");
					this.OnPreferredStateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PreferredLaborMarketArea", DbType="nvarchar(10)")]
		public string PreferredLaborMarketArea
		{
			get
			{
				return this._PreferredLaborMarketArea;
			}
			set
			{
				if ((this._PreferredLaborMarketArea != value))
				{
					this.OnPreferredLaborMarketAreaChanging(value);
					this.SendPropertyChanging();
					this._PreferredLaborMarketArea = value;
					this.SendPropertyChanged("PreferredLaborMarketArea");
					this.OnPreferredLaborMarketAreaChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Title", DbType="nvarchar(100)")]
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if ((this._Title != value))
				{
					this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._Title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Organization", DbType="nvarchar(100)")]
		public string Organization
		{
			get
			{
				return this._Organization;
			}
			set
			{
				if ((this._Organization != value))
				{
					this.OnOrganizationChanging(value);
					this.SendPropertyChanging();
					this._Organization = value;
					this.SendPropertyChanged("Organization");
					this.OnOrganizationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Address1", DbType="nvarchar(100)")]
		public string Address1
		{
			get
			{
				return this._Address1;
			}
			set
			{
				if ((this._Address1 != value))
				{
					this.OnAddress1Changing(value);
					this.SendPropertyChanging();
					this._Address1 = value;
					this.SendPropertyChanged("Address1");
					this.OnAddress1Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Address2", DbType="nvarchar(100)")]
		public string Address2
		{
			get
			{
				return this._Address2;
			}
			set
			{
				if ((this._Address2 != value))
				{
					this.OnAddress2Changing(value);
					this.SendPropertyChanging();
					this._Address2 = value;
					this.SendPropertyChanged("Address2");
					this.OnAddress2Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_City", DbType="nvarchar(100)")]
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				if ((this._City != value))
				{
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._City = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_State", DbType="nvarchar(10)")]
		public string State
		{
			get
			{
				return this._State;
			}
			set
			{
				if ((this._State != value))
				{
					this.OnStateChanging(value);
					this.SendPropertyChanging();
					this._State = value;
					this.SendPropertyChanged("State");
					this.OnStateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Zip", DbType="nvarchar(10)")]
		public string Zip
		{
			get
			{
				return this._Zip;
			}
			set
			{
				if ((this._Zip != value))
				{
					this.OnZipChanging(value);
					this.SendPropertyChanging();
					this._Zip = value;
					this.SendPropertyChanged("Zip");
					this.OnZipChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Phone", DbType="nvarchar(20)")]
		public string Phone
		{
			get
			{
				return this._Phone;
			}
			set
			{
				if ((this._Phone != value))
				{
					this.OnPhoneChanging(value);
					this.SendPropertyChanging();
					this._Phone = value;
					this.SendPropertyChanged("Phone");
					this.OnPhoneChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PhoneExtension", DbType="nvarchar(10)")]
		public string PhoneExtension
		{
			get
			{
				return this._PhoneExtension;
			}
			set
			{
				if ((this._PhoneExtension != value))
				{
					this.OnPhoneExtensionChanging(value);
					this.SendPropertyChanging();
					this._PhoneExtension = value;
					this.SendPropertyChanged("PhoneExtension");
					this.OnPhoneExtensionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SecureProperties", DbType="nvarchar(MAX)")]
		public string SecureProperties
		{
			get
			{
				return this._SecureProperties;
			}
			set
			{
				if ((this._SecureProperties != value))
				{
					this.OnSecurePropertiesChanging(value);
					this.SendPropertyChanging();
					this._SecureProperties = value;
					this.SendPropertyChanged("SecureProperties");
					this.OnSecurePropertiesChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CustomProperties", DbType="nvarchar(MAX)")]
		public string CustomProperties
		{
			get
			{
				return this._CustomProperties;
			}
			set
			{
				if ((this._CustomProperties != value))
				{
					this.OnCustomPropertiesChanging(value);
					this.SendPropertyChanging();
					this._CustomProperties = value;
					this.SendPropertyChanged("CustomProperties");
					this.OnCustomPropertiesChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ParticipantGroup")]
	public partial class ParticipantGroup : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private global::MS.Utility.ExtendedPropertyScopeType _ScopeType;
		
		private System.Nullable<int> _ScopeID;
		
		private System.DateTime _CreatedTimestamp;
		
		private System.DateTime _LastModifiedTimestamp;
		
		private string _Name;
		
		private string _Overview;
		
		private EntitySet<Participant> _Participants;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnScopeTypeChanging(global::MS.Utility.ExtendedPropertyScopeType value);
    partial void OnScopeTypeChanged();
    partial void OnScopeIDChanging(System.Nullable<int> value);
    partial void OnScopeIDChanged();
    partial void OnCreatedTimestampChanging(System.DateTime value);
    partial void OnCreatedTimestampChanged();
    partial void OnLastModifiedTimestampChanging(System.DateTime value);
    partial void OnLastModifiedTimestampChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnOverviewChanging(string value);
    partial void OnOverviewChanged();
    #endregion
		
		public ParticipantGroup()
		{
			this._Participants = new EntitySet<Participant>(new Action<Participant>(this.attach_Participants), new Action<Participant>(this.detach_Participants));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ScopeType", DbType="Int NOT NULL", CanBeNull=false)]
		public global::MS.Utility.ExtendedPropertyScopeType ScopeType
		{
			get
			{
				return this._ScopeType;
			}
			set
			{
				if ((this._ScopeType != value))
				{
					this.OnScopeTypeChanging(value);
					this.SendPropertyChanging();
					this._ScopeType = value;
					this.SendPropertyChanged("ScopeType");
					this.OnScopeTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ScopeID", DbType="Int")]
		public System.Nullable<int> ScopeID
		{
			get
			{
				return this._ScopeID;
			}
			set
			{
				if ((this._ScopeID != value))
				{
					this.OnScopeIDChanging(value);
					this.SendPropertyChanging();
					this._ScopeID = value;
					this.SendPropertyChanged("ScopeID");
					this.OnScopeIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreatedTimestamp", DbType="DATETIME2 NOT NULL")]
		public System.DateTime CreatedTimestamp
		{
			get
			{
				return this._CreatedTimestamp;
			}
			set
			{
				if ((this._CreatedTimestamp != value))
				{
					this.OnCreatedTimestampChanging(value);
					this.SendPropertyChanging();
					this._CreatedTimestamp = value;
					this.SendPropertyChanged("CreatedTimestamp");
					this.OnCreatedTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastModifiedTimestamp", DbType="DATETIME2 NOT NULL")]
		public System.DateTime LastModifiedTimestamp
		{
			get
			{
				return this._LastModifiedTimestamp;
			}
			set
			{
				if ((this._LastModifiedTimestamp != value))
				{
					this.OnLastModifiedTimestampChanging(value);
					this.SendPropertyChanging();
					this._LastModifiedTimestamp = value;
					this.SendPropertyChanged("LastModifiedTimestamp");
					this.OnLastModifiedTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="nvarchar(128) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Overview", DbType="nvarchar(MAX)")]
		public string Overview
		{
			get
			{
				return this._Overview;
			}
			set
			{
				if ((this._Overview != value))
				{
					this.OnOverviewChanging(value);
					this.SendPropertyChanging();
					this._Overview = value;
					this.SendPropertyChanged("Overview");
					this.OnOverviewChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ParticipantGroup_Participant", Storage="_Participants", ThisKey="ID", OtherKey="ParticipantGroupID")]
		public EntitySet<Participant> Participants
		{
			get
			{
				return this._Participants;
			}
			set
			{
				this._Participants.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Participants(Participant entity)
		{
			this.SendPropertyChanging();
			entity.ParticipantGroup = this;
		}
		
		private void detach_Participants(Participant entity)
		{
			this.SendPropertyChanging();
			entity.ParticipantGroup = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Participant")]
	public partial class Participant : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ID;
		
		private global::MS.Utility.ExtendedPropertyScopeType _ScopeType;
		
		private System.Nullable<int> _ScopeID;
		
		private System.DateTime _CreatedTimestamp;
		
		private System.DateTime _LastModifiedTimestamp;
		
		private System.Nullable<uint> _Grade;
		
		private string _Name;
		
		private string _Overview;
		
		private int _ParticipantGroupID;
		
		private EntityRef<ParticipantGroup> _ParticipantGroup;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIDChanging(int value);
    partial void OnIDChanged();
    partial void OnScopeTypeChanging(global::MS.Utility.ExtendedPropertyScopeType value);
    partial void OnScopeTypeChanged();
    partial void OnScopeIDChanging(System.Nullable<int> value);
    partial void OnScopeIDChanged();
    partial void OnCreatedTimestampChanging(System.DateTime value);
    partial void OnCreatedTimestampChanged();
    partial void OnLastModifiedTimestampChanging(System.DateTime value);
    partial void OnLastModifiedTimestampChanged();
    partial void OnGradeChanging(System.Nullable<uint> value);
    partial void OnGradeChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnOverviewChanging(string value);
    partial void OnOverviewChanged();
    partial void OnParticipantGroupIDChanging(int value);
    partial void OnParticipantGroupIDChanged();
    #endregion
		
		public Participant()
		{
			this._ParticipantGroup = default(EntityRef<ParticipantGroup>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				if ((this._ID != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._ID = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ScopeType", DbType="Int NOT NULL", CanBeNull=false)]
		public global::MS.Utility.ExtendedPropertyScopeType ScopeType
		{
			get
			{
				return this._ScopeType;
			}
			set
			{
				if ((this._ScopeType != value))
				{
					this.OnScopeTypeChanging(value);
					this.SendPropertyChanging();
					this._ScopeType = value;
					this.SendPropertyChanged("ScopeType");
					this.OnScopeTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ScopeID", DbType="Int")]
		public System.Nullable<int> ScopeID
		{
			get
			{
				return this._ScopeID;
			}
			set
			{
				if ((this._ScopeID != value))
				{
					this.OnScopeIDChanging(value);
					this.SendPropertyChanging();
					this._ScopeID = value;
					this.SendPropertyChanged("ScopeID");
					this.OnScopeIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreatedTimestamp", DbType="DATETIME2 NOT NULL")]
		public System.DateTime CreatedTimestamp
		{
			get
			{
				return this._CreatedTimestamp;
			}
			set
			{
				if ((this._CreatedTimestamp != value))
				{
					this.OnCreatedTimestampChanging(value);
					this.SendPropertyChanging();
					this._CreatedTimestamp = value;
					this.SendPropertyChanged("CreatedTimestamp");
					this.OnCreatedTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LastModifiedTimestamp", DbType="DATETIME2 NOT NULL")]
		public System.DateTime LastModifiedTimestamp
		{
			get
			{
				return this._LastModifiedTimestamp;
			}
			set
			{
				if ((this._LastModifiedTimestamp != value))
				{
					this.OnLastModifiedTimestampChanging(value);
					this.SendPropertyChanging();
					this._LastModifiedTimestamp = value;
					this.SendPropertyChanged("LastModifiedTimestamp");
					this.OnLastModifiedTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Grade", DbType="Int")]
		public System.Nullable<uint> Grade
		{
			get
			{
				return this._Grade;
			}
			set
			{
				if ((this._Grade != value))
				{
					this.OnGradeChanging(value);
					this.SendPropertyChanging();
					this._Grade = value;
					this.SendPropertyChanged("Grade");
					this.OnGradeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="nvarchar(256)")]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Overview", DbType="nvarchar(MAX)")]
		public string Overview
		{
			get
			{
				return this._Overview;
			}
			set
			{
				if ((this._Overview != value))
				{
					this.OnOverviewChanging(value);
					this.SendPropertyChanging();
					this._Overview = value;
					this.SendPropertyChanged("Overview");
					this.OnOverviewChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParticipantGroupID", DbType="Int NOT NULL")]
		public int ParticipantGroupID
		{
			get
			{
				return this._ParticipantGroupID;
			}
			set
			{
				if ((this._ParticipantGroupID != value))
				{
					if (this._ParticipantGroup.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnParticipantGroupIDChanging(value);
					this.SendPropertyChanging();
					this._ParticipantGroupID = value;
					this.SendPropertyChanged("ParticipantGroupID");
					this.OnParticipantGroupIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ParticipantGroup_Participant", Storage="_ParticipantGroup", ThisKey="ParticipantGroupID", OtherKey="ID", IsForeignKey=true)]
		public ParticipantGroup ParticipantGroup
		{
			get
			{
				return this._ParticipantGroup.Entity;
			}
			set
			{
				ParticipantGroup previousValue = this._ParticipantGroup.Entity;
				if (((previousValue != value) 
							|| (this._ParticipantGroup.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ParticipantGroup.Entity = null;
						previousValue.Participants.Remove(this);
					}
					this._ParticipantGroup.Entity = value;
					if ((value != null))
					{
						value.Participants.Add(this);
						this._ParticipantGroupID = value.ID;
					}
					else
					{
						this._ParticipantGroupID = default(int);
					}
					this.SendPropertyChanged("ParticipantGroup");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591