app.constant('IDENTITY_TYPE', {
  tenant: 'tenant',
  user: 'user'
});

app.run(['$rootScope', '$state', '$stateParams', '$http', '$templateCache', 'utilityService', 'siteService', 'AUTHORIZATION_ROLES', function ($rootScope, $state, $stateParams, $http, $templateCache, utilityService, siteService, AUTHORIZATION_ROLES) {
  // (Add a reference to siteService, so it loads early and we can register our .on handlers
}]);

//This handles retrieving data and is used by controllers. 3 options (server, factory, provider) with
//each doing the same thing just structuring the functions/data differently.
app.service('siteService', ['$rootScope', '$q', '$state', 'utilityService', 'msIdentity', 'msAuthenticated', 'TEMPLATE_URL', 'CONNECTION_EVENT', function ($rootScope, $q, $state, utilityService, msIdentity, msAuthenticated, TEMPLATE_URL, CONNECTION_EVENT) {

  var siteHub = $.connection.siteHub; // the generated client-side hub proxy
  var self = this;

  // We want to share a common 'model' with the utilityService. So add our properties there.
  var model = utilityService.getModel();
  // Re-expose the model for callers that want to get it from us instead of utilityService
  this.getModel = function () {
    return model;
  };
  this.model = model;


  this.openUserModalDialog = function (theUser) {
    $modal.open({
      templateUrl: TEMPLATE_URL.client_partials_admin_userModalDialog,
      controller: UserModalController,
      resolve: {
        theUser: function () {
          return theUser;
        }
      }
    });
  }


  //***** SiteHub Methods *****

  //** Tenant CRUD

  this.createTenant = function (tenantName, data) {
    return utilityService.callHub(function () {
      return siteHub.server.createTenant(tenantName, data);
    });
  };

  this.createGroup = function (tenantName, parentTenantGroup, data) {
    return utilityService.callHub(function () {
      return siteHub.server.createGroup(tenantName, parentTenantGroup.id, data);
    });
  };


  //** Tenant Settings
  this.getSettings = function (tenant) {
    return utilityService.callHub(function () {
      return siteHub.server.getSettings(tenant.id);
    }).then(function (settingsObject) {
      onSettingsUpdated(settingsObject);
      return settingsObject;
    });
  };

  function onSettingsUpdated(settingsObject) {
    $rootScope.$broadcast('onSettingsUpdated:', settingsObject);
  };

  this.addTeamUrlResource = function (categoryID, name, url) {
    return utilityService.callHub(function () {
      return siteHub.server.addTeamUrlResource(categoryID, name, url);
    });
  };

  this.removeResource = function (id) {
    return utilityService.callHub(function () {
      return siteHub.server.removeResource(id);
    });
  };


  //** User and User Invites CRUD

  this.createJobSeeker = function (data) {
    return utilityService.callHub(function () {
      return siteHub.server.createJobSeeker(data);
    });
  };


  this.createDemoUser = function (appRole) {
    return utilityService.callHub(function () {
      return siteHub.server.createDemoUser(appRole);
    });
  };



  this.createDemoJobSeeker = function () {
    return utilityService.callHub(function () {
      return siteHub.server.createDemoJobSeeker();
    });
  };



  this.sendUserSignupInvitation = function (inviteeEmail, data) {
    return utilityService.callHub(function () {
      return siteHub.server.sendUserSignupInvitation(inviteeEmail, data);
    });
  };


  this.redeemUserSignupInvitation = function (createUserAuthCode, password, options) {
    return utilityService.callHub(function () {
      return siteHub.server.redeemUserSignupInvitation(createUserAuthCode, password, options);
    });
  };



  this.getEventSessionVolunteerAuthInfo = function (eventSession) {
    return utilityService.callHub(function () {
      return siteHub.server.getEventSessionVolunteerAuthInfo(eventSession.id);
    });
  };







  //** Events Related
  self.events = utilityService.createModelItems(siteHub.server.searchEvents);
  model.events = self.events;

  siteHub.on('updateEvents', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEvents', utilityService.updateItemsModel(self.events, itemsData)));
  });


  this.generateRandomEvent = function () {
    return utilityService.callHub(function () {
      return siteHub.server.generateRandomEvent();
    });
  };

  this.createEvent = function (formData) {
    return utilityService.callHub(function () {
      return siteHub.server.createEvent(formData);
    });
  };

  this.editEvent = function (eventID, eventData) {
    return utilityService.callHub(function () {
      return siteHub.server.editEvent(eventID, eventData);
    });
  };

  this.deleteEvent = function (event) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteEvent(event.id);
    });
  };

  this.modifyEventTag = function (item, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyEventTag(item.id, newTag, isAssigned);
    });
  };

  this.modifyEventMyTag = function (item, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyEventMyTag(item.id, newTag, isAssigned);
    });
  };






  //** EventSessions Related
  self.eventSessions = utilityService.createModelItems(siteHub.server.searchEventSessions);
  model.eventSessions = self.eventSessions;

  siteHub.on('updateEventSessions', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEventSessions', utilityService.updateItemsModel(self.eventSessions, itemsData)));
  });



  this.ensureAllEventSessions = function () {
    return model.eventSessions.search("", "", 0, 999999)
      .then(function (itemsData) {
        return model.eventSessions;
      });
  };

  this.createEventSession = function (eventSessionData) {
    return utilityService.callHub(function () {
      return siteHub.server.createEventSession(eventSessionData);
    });
  };

  this.editEventSession = function (eventSessionID, eventSessionData) {
    return utilityService.callHub(function () {
      return siteHub.server.editEventSession(eventSessionID, eventSessionData);
    });
  };

  this.deleteEventSession = function (eventSession) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteEventSession(eventSession.id);
    });
  };

  self.setEventSessionCheckInOpen = function (eventSession, isOpen) {
    return utilityService.callHub(function () {
      return siteHub.server.setEventSessionCheckInOpen(eventSession.id, isOpen);
    });
  };







  //** EventParticipants Related
  self.eventParticipants = utilityService.createModelItems(siteHub.server.searchEventParticipants);
  model.eventParticipants = self.eventParticipants;

  siteHub.on('updateEventParticipants', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEventParticipants', utilityService.updateItemsModel(self.eventParticipants, itemsData)));
  });

  self.eventParticipants.getSet = function (searchExpression) {
    return utilityService.callHub(function () {
      return siteHub.server.getEventParticipantSet(searchExpression);
    });
  };

  self.parseEventParticipants = function (event, data) {
    return utilityService.callHub(function () {
      return siteHub.server.parseEventParticipants(event.id, data);
    });
  };

  self.uploadEventParticipants = function (event, data) {
    return utilityService.callHub(function () {
      return siteHub.server.uploadEventParticipants(event.id, data);
    });
  };

  self.createEventParticipant = function (event, data) {
    return utilityService.callHub(function () {
      return siteHub.server.createEventParticipant(event.id, data);
    });
  };

  self.editEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.editEventParticipant(item.id, item);
    });
  };

  //deleteEventParticipant
  this.deleteEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteEventParticipant(item.id);
    });
  };
  this.deleteEventParticipants = function (itemIDs) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteEventParticipants(itemIDs);
    });
  };


  self.checkInEventParticipants = function (itemIDs) {
    return utilityService.callHub(function () {
      return siteHub.server.checkInEventParticipants(itemIDs);
    });
  };

  self.undoCheckInEventParticipants = function (itemIDs) {
    return utilityService.callHub(function () {
      return siteHub.server.undoCheckInEventParticipants(itemIDs);
    });
  };

  self.bulkEditEventParticipants = function (itemIDs, eventSessionID) {
    return utilityService.callHub(function () {
      return siteHub.server.bulkEditEventParticipants(itemIDs, eventSessionID);
    });
  };

  self.checkInEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.checkInEventParticipant(item.id);
    });
  };

  self.undoCheckInEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.undoCheckInEventParticipant(item.id);
    });
  };

  self.checkOutEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.checkOutEventParticipant(item.id);
    });
  };




























  this.modifyIsBetaUser = function (userID, isBetaUser) {
    model.users[userID].isBetaUser = isBetaUser;
    return utilityService.callHub(function () {
      return utilityHub.server.modifyIsBetaUser(userID, isBetaUser);
    });
  };




  this.doCommand = function (command, commandData) {
    return utilityService.callHub(function () {
      return siteHub.server.doCommand(command, commandData);
    });
  };










  // ** Participant Group related
  self.participantGroups = utilityService.createModelItems(siteHub.server.searchParticipantGroups);
  model.participantGroups = self.participantGroups;

  siteHub.on('updateParticipantGroups', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateParticipantGroups', utilityService.updateItemsModel(self.participantGroups, itemsData)));
  });


  self.parseParticipantGroups = function (event, data) {
    return utilityService.callHub(function () {
      return siteHub.server.parseParticipantGroups(event.id, data);
    });
  };

  self.uploadParticipantGroups = function (event, data) {
    return utilityService.callHub(function () {
      return siteHub.server.uploadParticipantGroups(event.id, data);
    });
  };



  this.createParticipantGroup = function (formData) {
    return utilityService.callHub(function () {
      return siteHub.server.createParticipantGroup(formData);
    });
  };

  this.editParticipantGroup = function (participantGroupID, participantGroupData) {
    return utilityService.callHub(function () {
      return siteHub.server.editParticipantGroup(participantGroupID, participantGroupData);
    });
  };

  this.deleteParticipantGroup = function (participantGroup) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteParticipantGroup(participantGroup.id);
    });
  };
  

  this.generateRandomParticipants = function (participantGroupID, numberOfParticipants) {
    return utilityService.callHub(function () {
      return siteHub.server.generateRandomParticipants(participantGroupID, numberOfParticipants);
    });
  }

  this.updateParticipantGroups = function (itemsData) {
    $rootScope.$apply(utilityService.updateItemsModel(model.participantGroups, itemsData));
  }


  this.generateRandomEvent = function () {
    return utilityService.callHub(function () {
      return siteHub.server.generateRandomEvent();
    });
  };






  // ** Particpants related
  model.participants = {
    hashMap: {},
    index: [],
    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchParticipants(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.participants, itemsData);
      });
    }
  };

  this.createParticipant = function (formData) {
    return utilityService.callHub(function () {
      return siteHub.server.createParticipant(formData);
    });
  };

  this.updateParticipants = function (itemsData) {
    $rootScope.$apply(utilityService.updateItemsModel(model.participants, itemsData));
  };




  siteHub.on('updateSettings', function (settingsObject) {
    $rootScope.$apply(onSettingsUpdated(settingsObject));
  }).on('setAuthenticatedEventSession', function (itemsData) {


    // please hit me...
    $rootScope.$apply(onSetAuthenticatedEventSession(itemsData));



  }).on('setAuthenticatedItemUserSession', function (itemsData) {


    // please hit me...
    $rootScope.$apply(onSetAuthenticatedItemUserSession(itemsData));



  }).on('updateParticipants', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateParticipants', utilityService.updateItemsModel(model.participants, itemsData)));


  }).on('updateProgress', function (progressData) {
    $rootScope.$apply($rootScope.$broadcast('updateProgress', progressData));
  });

  function onSetAuthenticatedEventSession(itemsData) {
    // usersData is expected to contain just one user - the authenticated user
    //onUsersUpdated(usersData);
    if (itemsData && itemsData.items) {

      //!! TODO - this users' data might change - need to track that in onUsersUpdated() - but do that after we change the server notification
      var authenticatedItem = itemsData.items[0];
      var appRoles = ['EventSessionVolunteer'];
      var systemRoles = [];
      model.authenticatedIdentity = msIdentity.create('eventSession', authenticatedItem.id, authenticatedItem.name, appRoles, systemRoles, authenticatedItem.profilePhotoUrl);
      model.authenticatedUser = null;

      msAuthenticated.setAuthenticatedIdentity(model.authenticatedIdentity);


      // head to Kiosk home page
      $state.go('app.spa-landing', {}, { reload: true });

      $rootScope.$broadcast('authenticated:', model.authenticatedIdentity);
    }
  };

  function onSetAuthenticatedItemUserSession(itemUserData) {
    if (itemUserData) {

      var appRoles = itemUserData.appRoles;
      var systemRoles = itemUserData.systemRoles;
      var displayName = itemUserData.displayName;
      var profilePhotoUrl = itemUserData.profilePhotoUrl;

      var userID = itemUserData.userID;

      var itemType = itemUserData.itemType;
      var itemID = itemUserData.itemID;

      // If we're authenticated, we have an authenticatedIdentity
      model.authenticatedIdentity = msIdentity.createItemUser(appRoles, systemRoles, displayName, profilePhotoUrl, userID, itemType, itemID);
      if (userID) {
        // But only users have authenticatedUser
        model.authenticatedUser = model.authenticatedIdentity;
      }

      msAuthenticated.setAuthenticatedIdentity(model.authenticatedIdentity);


      // head to Kiosk home page
      $state.go('app.spa-landing', {}, { reload: true });

      $rootScope.$broadcast('authenticated:', model.authenticatedIdentity);
    }
  };






  $rootScope.$on(CONNECTION_EVENT.connectionStarting, function () {
    // Starting a new connection - ensure we don't leak any data fetched by the last user/connection
    model.cases = {};
    model.casesIndex = [];
    // unsorted
    model.activeCasesIndex = [];
    model.blockedCasesIndex = [];   // Blocked cases returned to Technologist
    // sorted by closedTimestamp
    model.closedCasesIndex = [];
    model.closedCasesTotalCount = null;
  });
  $rootScope.$on(CONNECTION_EVENT.connectionStarted, function () {
    //!! in TORQ I don't think we want to get all users - aren't there too many of them?
    //utilityService.getUsers();
  });


  // init
}]);


app.filter('demandObjects', function () {
  return function (objects, demandObject) {
    var resultObjects = [];
    angular.forEach(objects, function (object) {
      resultObjects.push(demandObject(object));
    });
    return resultObjects;
  }
});

