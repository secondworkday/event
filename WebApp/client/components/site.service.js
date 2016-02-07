app.constant('IDENTITY_TYPE', {
  tenant: 'tenant',
  user: 'user'
});

//This handles retrieving data and is used by controllers. 3 options (server, factory, provider) with
//each doing the same thing just structuring the functions/data differently.
app.service('siteService', ['$rootScope', '$q', '$state', 'utilityService', 'msIdentity', 'TEMPLATE_URL', 'CONNECTION_EVENT', function ($rootScope, $q, $state, utilityService, msIdentity, TEMPLATE_URL, CONNECTION_EVENT) {

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





  self.searchCompanyLayTitle = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchCompanyLayTitle(searchExpression, sortExpression, startIndex, rowCount);
    });
  };

  function onCompanyLayTitlesUpdated(itemsData) {

    var notification = {
      ids: $.map(itemsData.items, function (item) {
        return item.id;
      }),
      deletedIDs: itemsData.deletedIDs,
      totalCount: itemsData.totalCount,
      resolvedIDs: []
    };

    return notification;
  };




  self.setCompanyLayTitleNoMatch = function (itemID) {
    return utilityService.callHub(function () {
      return siteHub.server.setCompanyLayTitleNoMatch(itemID);
    });
  };

  self.setCompanyLayTitleMatch = function (itemID, companyName) {
    return utilityService.callHub(function () {
      return siteHub.server.setCompanyLayTitleMatch(itemID, companyName);
    });
  };






  //** Occupation Related

  model.occupations = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchOccupations(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.occupations, itemsData);
      });
    }
  };


  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
  self.ensureOccupation = function (itemID) {
    var modelItems = model.occupations;
    var item = modelItems.hashMap[itemID];
    if (!item) {
      return modelItems.search("%" + itemID, "", 0, 1)
      .then(function (ignoredNotificationData) {
        return modelItems.hashMap[itemID];
      });
    }

    return $q.when(item);
  };

  // returns an Item (not a promise - see ensureXyz() for the promise variant)
  self.demandOccupation = function (itemKey) {
    var modelItems = model.occupations;
    return modelItems.hashMap[itemKey] ||
      (
        modelItems.hashMap[itemKey] = { key: itemKey, id: itemKey, code: itemKey, displayTitle: 'loading...' },
        utilityService.delayLoad2(modelItems, itemKey),
        modelItems.hashMap[itemKey]
      );
  };

  self.setOccupationTitle = function (occupation, title) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationTitle(occupation.onetCode, title);
    });
  };

  self.setOccupationDescription = function (occupation, description) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationDescription(occupation.onetCode, description);
    });
  };

  self.setOccupationReleaseState = function (occupation, releaseState) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationReleaseState(occupation.onetCode, releaseState);
    });
  };

  self.setOccupationHeroImageLicense = function (occupation, license) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationHeroImageLicense(occupation.onetCode, license);
    });
  };

  self.setOccupationHeroImageSource = function (occupation, source) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationHeroImageSource(occupation.onetCode, source);
    });
  };

  self.setOccupationHeroImageFocalPoint = function (occupation, x, y) {
    return utilityService.callHub(function () {
      return siteHub.server.setOccupationHeroImageFocalPoint(occupation.onetCode, x, y);
    });
  };




  self.createOccupationBundle = function (name, options) {
    return utilityService.callHub(function () {
      return siteHub.server.createOccupationBundle(name, options);
    });
  };

  self.deleteOccupationBundle = function (occupationBundle) {
    return utilityService.callHub(function () {
      return siteHub.server.deleteOccupationBundle(occupationBundle.id);
    });
  };

  self.searchOccupationBundles = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchOccupationBundles(searchExpression, sortExpression, startIndex, rowCount);
    });
  };

  self.modifyOccupationBundleMember = function (occupationBundle, occupation, isMember) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyOccupationBundleMember(occupationBundle.id, occupation.onetCode, isMember);
    });
  };











  //** Job Seeker Profile Related
  model.jsProfiles = {};
  model.jsProfilesIndex = [];

  this.addProfileOccupation = function (profile, occupation) {

    profile.nextOccupationIDs.push(occupation.id);

    //return utilityService.callHub(function () {
    //      return siteHub.server.modifyProjectTag(project.id, newTag, isAssigned);
    //});
  };










  //** CareerProfiles (aka JobSeeker) Related
  model.careerProfiles = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchCareerProfiles(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.careerProfiles, itemsData);
      });
    }
  };

  self.getCareerProfile = function (careerProfileID) {
    return utilityService.callHub(function () {
      return siteHub.server.getCareerProfile(careerProfileID);
    }).then(function (itemsData) {
      utilityService.updateItemsModel(model.careerProfiles, itemsData);

      var item = model.careerProfiles.hashMap[careerProfileID];
      if (item) {
        return item;
      }
      return $q.reject("career profile not found");
    });
  };


  this.modifyCareerProfileTag = function (client, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyCareerProfileTag(client.id, newTag, isAssigned);
    });
  };
  this.modifyCareerProfileMyTag = function (client, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyCareerProfileMyTag(client.id, newTag, isAssigned);
    });
  };

















  //** Career Step Related

  model.careerSteps = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchCareerSteps(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.careerSteps, itemsData);
      });
    }
  };


  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
  self.ensureCareerStep = function (itemID) {
    var modelItems = model.careerSteps;
    var item = modelItems.hashMap[itemID];
    if (!item) {
      return modelItems.search("%" + itemID, "", 0, 1)
      .then(function (ignoredNotificationData) {
        return modelItems.hashMap[itemID];
      });
    }

    return $q.when(item);
  };

  // returns an Item (not a promise - see ensureXyz() for the promise variant)
  self.demandCareerStep = function (itemID) {
    var modelItems = model.careerSteps;
    return modelItems.hashMap[itemID] ||
      (
        modelItems.hashMap[itemID] = { id: itemID, code: itemID, displayTitle: 'loading...' },
        utilityService.delayLoad2(modelItems, itemID),
        modelItems.hashMap[itemID]
      );
  };

  this.setCareerStepZipCode = function (careerStep, zipCode) {
    //careerStep.zipCode = zipCode;
    //!! tell the server what just happened here...
    return utilityService.callHub(function () {
      return siteHub.server.setCareerStepZipCode(careerStep.id, zipCode);
    });
  };

  this.modifyCareerStepSettings = function (careerStep, settings) {
    //careerStep.zipCode = zipCode;
    //!! tell the server what just happened here...
    return utilityService.callHub(function () {
      return siteHub.server.modifyCareerStepSettings(careerStep.id, settings);
    });
  };



  self.addCareerStepCareerHistoryWork = function (careerStep, workPeriod) {
    return utilityService.callHub(function () {
      return siteHub.server.addCareerStepCareerHistoryWork(careerStep.id, workPeriod);
    });
  };
  self.removeCareerStepCareerHistoryWork = function (careerStep, workPeriodIndex) {
    return utilityService.callHub(function () {
      return siteHub.server.removeCareerStepCareerHistoryWork(careerStep.id, workPeriodIndex);
    });
  };
  self.modifyCareerStepCareerHistoryWork = function (careerStep, workPeriodIndex, workPeriod) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyCareerStepCareerHistoryWork(careerStep.id, workPeriodIndex, workPeriod);
    });
  };



  self.addCareerStepOccupation = function (careerStep, occupation) {
    return utilityService.callHub(function () {
      return siteHub.server.addCareerStepOccupation(careerStep.id, occupation.onetCode);
    });
  };
  self.removeCareerStepOccupation = function (careerStep, occupation) {
    return utilityService.callHub(function () {
      return siteHub.server.removeCareerStepOccupation(careerStep.id, occupation.onetCode);
    });
  };
  self.skipCareerStepOccupation = function (careerStep, occupation) {
    return utilityService.callHub(function () {
      return siteHub.server.skipCareerStepOccupation(careerStep.id, occupation.onetCode);
    });
  };

  self.getCareerStepJobOpenings2 = function (careerStep, occupation, zipCode, searchRadius) {
    return utilityService.callHub(function () {
      return siteHub.server.getCareerStepJobOpenings2(careerStep.id, occupation.onetCode, zipCode, searchRadius);
    });
  };
  // note - we identify a "JobOpportunity" via a "JobPosting"
  self.addCareerStepJobOpportunity = function (careerStep, occupation, jobPosting) {
    return utilityService.callHub(function () {
      return siteHub.server.addCareerStepJobOpportunity(careerStep.id, occupation.onetCode, jobPosting);
    });
  };
  self.removeCareerStepJobOpportunity = function (careerStep, jobPosting) {
    return utilityService.callHub(function () {
      return siteHub.server.removeCareerStepJobOpportunity(careerStep.id, jobPosting);
    });
  };
  self.skipCareerStepJobOpening = function (careerStep, jobOpening) {
    return utilityService.callHub(function () {
      // Note server expects JobPosting
      return siteHub.server.skipCareerStepJobPosting(careerStep.id, jobOpening.jobPosting);
    });
  };




  self.getCareerStepEducationOptions = function (careerStep, occupation, zipCode, searchRadius) {
    return utilityService.callHub(function () {
      return siteHub.server.getCareerStepEducationOptions(careerStep.id, occupation.onetCode, zipCode, searchRadius);
    });
  };
  // note - we identify a "JobOpportunity" via a "JobPosting"
  self.addCareerStepEducationOpportunity = function (careerStep, occupation, educationProgram) {
    return utilityService.callHub(function () {
      return siteHub.server.addCareerStepEducationOpportunity(careerStep.id, occupation.onetCode, educationProgram);
    });
  };
  self.removeCareerStepEducationOpportunity = function (careerStep, educationProgram) {
    return utilityService.callHub(function () {
      return siteHub.server.removeCareerStepEducationOpportunity(careerStep.id, educationProgram);
    });
  };
  self.skipCareerStepEducationOpportunity = function (careerStep, educationProgram) {
    return utilityService.callHub(function () {
      // Note server expects JobPosting
      return siteHub.server.skipCareerStepEducationOpportunity(careerStep.id, educationProgram);
    });
  };







  self.searchCareerStepTransitions = function (careerStep, searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchCareerStepTransitions(careerStep.id, searchExpression, sortExpression, startIndex, rowCount);
    });
  };



  //** EventSessions Related
  model.eventSessions = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchEventSessions(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.eventSessions, itemsData);
      });
    }
  };

  this.ensureEventSessions = function (eventID) {
    // Find EventSessions that belong to EventID (foreign key)
    // Always load latest from server because we can't know if we have them all on the client
    return model.eventSessions.search("", "", 0, 999999)    // TODO: search("EventID:" + eventID, ...
      .then(function (itemsData) {
        // TODO: doing client-side filtering for now, but filtering should be done server-side
        return $.map(model.eventSessions.hashMap, function (value, index) {
          if (eventID == value.eventID) {
            return value.id;
          }
        });
      });
  };




  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
  self.ensureEventSession = function (itemID) {
    var modelItems = model.eventSessions;
    var item = modelItems.hashMap[itemID];
    if (!item) {
      return modelItems.search("%" + itemID, "", 0, 1)
      .then(function (ignoredNotificationData) {
        return modelItems.hashMap[itemID];
      });
    }

    return $q.when(item);
  };

  // returns an Item (not a promise - see ensureXyz() for the promise variant)
  self.demandEventSession = function (itemKey) {
    var modelItems = model.eventSessions;
    return modelItems.hashMap[itemKey] ||
      (
        modelItems.hashMap[itemKey] = { key: itemKey, id: itemKey, code: itemKey, displayTitle: 'loading...' },
        utilityService.delayLoad2(modelItems, itemKey),
        modelItems.hashMap[itemKey]
      );
  };


  self.setEventSessionState = function (eventSession, stateName) {
    return utilityService.callHub(function () {
      return siteHub.server.setEventSessionState(eventSession.id, stateName);
    });
  };






  //** EventSessions Related
  model.eventParticipants = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchEventParticipants(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.eventParticipants, itemsData);
      });
    }
  };

  //this.ensureEventParticipants = function (eventID) {
  //  // Find EventParticipants that belong to EventID (foreign key)
  //  // Always load latest from server because we can't know if we have them all on the client
  //  return model.eventParticipants.search("", "", 0, 999999)    // TODO: search("EventID:" + eventID, ...
  //    .then(function (itemsData) {
  //      return model.participants.search("", "", 0, 999999)     // TODO: Need a EventParticipant-Participant JOIN on the server!!!
  //        .then(function (itemsData) {
  //          // TODO: doing client-side filtering for now, but filtering should be done server-side
  //          return $.map(model.eventParticipants.hashMap, function (value, index) {
  //            if (eventID == value.eventID) {
  //              var participant = model.participants.hashMap[value.participantID];
  //              value.name = participant.name;
  //              value.participantGroupID = participant.participantGroupID;
  //              return value;
  //            }
  //          });
  //        });
  //    });
  //};

  this.ensureEventParticipantsIndex = function (eventID) {
    // Find EventParticipants that belong to EventID (foreign key)
    // Always load latest from server because we can't know if we have them all on the client
    return model.eventParticipants.search("", "", 0, 999999)    // TODO: search("EventID:" + eventID, ...
      .then(function (itemsData) {
        return model.participants.search("", "", 0, 999999)     // TODO: Need a EventParticipant-Participant JOIN on the server!!!
          .then(function (itemsData) {
            // TODO: doing client-side filtering for now, but filtering should be done server-side
            return $.map(model.eventParticipants.hashMap, function (value, index) {
              if (eventID == value.eventID) {
                return value.id;
              }
            });
          });
      });
  }


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



  self.checkInEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.checkInEventParticipant(item.id);
    });
  };
  self.checkOutEventParticipant = function (item) {
    return utilityService.callHub(function () {
      return siteHub.server.checkOutEventParticipant(item.id);
    });
  };















  //** Events Related
  model.events = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchEvents(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.events, itemsData);
      });
    }
  };

  this.ensureEvent = function (eventID) {
    var event = model.events.hashMap[eventID];
    if (!event) {
      return model.events.search("%" + eventID, "", 0, 999999)
      .then(function (eventData) {
        return model.events.hashMap[eventID];
      });
    }

    return $q.when(event);
  }

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



  self.getEvent = function (itemID) {
    var searchExpression = "%" + itemID;
    //!! hmm - perhaps need a flag to indicate we want detailed information, not just overview information
    return self.searchEvents(searchExpression, '', 0, 1)
    .then(function (itemsData) {
      var item = model.events.hashMap[itemID];
      if (item) {
        return item;
      }
      return $q.reject("not found");
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


































  //** Clients (aka JobSeeker) Related
  model.clients = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchClients(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.clients, itemsData);
      });
    }
  };


  self.getClient = function (clientID) {
    var searchExpression = "%" + clientID;
    //!! hmm - perhaps need a flag to indicate we want detailed information, not just overview information
    return self.searchClients(searchExpression, '', 0, 1)
    .then(function (itemsData) {
      var item = model.clients.hashMap[clientID];
      if (item) {
        return item;
      }
      return $q.reject("not found");
    });
  };

  self.searchClients = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchClients(searchExpression, sortExpression, startIndex, rowCount);
    }).then(function (itemsData) {
      return onClientsUpdated(itemsData);
    });
  };

  this.modifyClientTag = function (client, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyClientTag(client.id, newTag, isAssigned);
    });
  };
  this.modifyClientMyTag = function (client, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyClientMyTag(client.id, newTag, isAssigned);
    });
  };



  //** Project Related
  model.projects = {};
  model.projectsIndex = [];


  function onProjectsUpdated(itemsData, isExternalEvent) {

    //!! old one
    //utilityService.purgeItems(projectsData.deletedIDs, model.projects, model.projectsIndex);
    //utilityService.cacheItems(projectsData.items, model.projects, model.projectsIndex);
    //model.projectsTotalCount = projectsData.totalCount;

    // Update our main cache (model.issues) and cacheIndex (model.issuesIndex)
    utilityService.purgeItems(itemsData.deletedIDs, model.projects, model.projectsIndex);
    var newItemIDs = utilityService.cacheItems(itemsData.items, model.projects, model.projectsIndex);

    var notification = {
      hashMap: model.projects,
      ids: $.map(itemsData.items, function (item) {
        return item.id;
      }),
      newIDs: newItemIDs,
      deletedIDs: itemsData.deletedIDs,
      resolvedIDs: []
    };

    if (!isExternalEvent) {
      notification.totalCount = itemsData.totalCount;
    }

    if (isExternalEvent) {
      $rootScope.$broadcast('updateProjects', notification);
    }

    return notification;
  };

  this.searchProjects = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchProjects(searchExpression, sortExpression, startIndex, rowCount);
    }).then(function (projectsData) {
      return onProjectsUpdated(projectsData);
    });
  };



  this.getProjects = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      var projects = siteHub.server.searchProjects(searchExpression, sortExpression, startIndex, rowCount);
      return projects;
    }).then(function (projectsData) {
      onProjectsUpdated(projectsData);
      return projectsData;
    });
  };

  this.getFavoriteProjects = function (startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.getFavoriteProjects(startIndex, rowCount);
    }).then(function (projectsData) {
      onProjectsUpdated(projectsData);
      return projectsData;
    });
  };


  this.modifyProjectTag = function (project, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyProjectTag(project.id, newTag, isAssigned);
    });
  };
  this.modifyProjectMyTag = function (project, newTag, isAssigned) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyProjectMyTag(project.id, newTag, isAssigned);
    });
  };
  this.modifyMyFavoriteProject = function (project, isFavorite) {
    return utilityService.callHub(function () {
      return siteHub.server.modifyMyFavoriteProject(project.id, isFavorite);
    });
  };



  this.migrateProject = function (project) {
    return utilityService.callHub(function () {
      return siteHub.server.migrateProject(project.id);
    });
  };
  this.migrateDemoProject = function (project) {
    return utilityService.callHub(function () {
      return siteHub.server.migrateDemoProject(project.id);
    });
  };


  this.emailProjectReport = function (project, emailType, mailMessage) {
    return utilityService.callHub(function () {
      return siteHub.server.emailProjectReport(project.id, emailType, mailMessage);
    });
  };



  // Transition Releated 



  this.modifyIsBetaUser = function (userID, isBetaUser) {
    model.users[userID].isBetaUser = isBetaUser;
    return utilityService.callHub(function () {
      return utilityHub.server.modifyIsBetaUser(userID, isBetaUser);
    });
  };


  this.getZipCodeInfo = function (zipCode) {
    return utilityService.callHub(function () {
      return siteHub.server.getZipCodeInfo(zipCode);
    });
  };



  this.getTouchOccupations = function (occupationCode, zipCode) {
    var deferred = utilityService.callHub(function () {
      return siteHub.server.getTouchOccupations(occupationCode, zipCode);
    }).then(function (touchData) {
      model.touch = touchData;
      return touchData;
    });

    return deferred;
  };


  function onTouchData(touchData) {
    model.touch = touchData;
  };



  self.startAutocoderThroughputTestTask = function (parameters) {
    return utilityService.callHub(function () {
      return siteHub.server.startAutocoderThroughputTestTask(parameters);
    }).then(function (taskID) {

      var taskItem = {
        taskID: taskID
      };

      return taskItem;
    });
  };



  self.getJobOpenings = function (occupationCode, zipCode, searchRadius, jobBoardCodes) {
    return utilityService.callHub(function () {
      return siteHub.server.getJobOpenings(occupationCode, zipCode, searchRadius, jobBoardCodes);
    });
  };


  this.getTouchJobs = function (occupationCode, zipCode, searchRadius) {
    var deferred = utilityService.callHub(function () {
      return siteHub.server.getTouchJobs(occupationCode, zipCode, searchRadius);
    });
    return deferred;
  };


  this.getEducationInfo = function () {
    return utilityService.callHub(function () {
      return siteHub.server.getEducationInfo();
    });
  };


  this.getEducationPrograms = function (occupationCode, zipCode) {
    return utilityService.callHub(function () {
      return siteHub.server.getEducationPrograms(occupationCode, zipCode);
    });
  };

  this.getIpedsEducationPrograms = function (occupationCode, zipCode) {
    return utilityService.callHub(function () {
      return siteHub.server.getIpedsEducationPrograms(occupationCode, zipCode);
    });
  };

  this.searchIpedsEducationPrograms = function (referenceDatabaseNamePrefix, searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.callHub(function () {
      return siteHub.server.searchIpedsEducationPrograms(referenceDatabaseNamePrefix, searchExpression, sortExpression, startIndex, rowCount);
    });
  };




  this.getOccupationInfo = function (referenceDatabaseNamePrefix, searchTerm, limit) {
    var deferred = utilityService.callHub(function () {
      return siteHub.server.getOccupationInfo(referenceDatabaseNamePrefix, searchTerm, limit);
    }).then(function (touchData) {
      return touchData;
    });

    return deferred;
  };

  this.getRandomLayTitle = function (searchTerm) {
    return utilityService.callHub(function () {
      return siteHub.server.getRandomLayTitle(searchTerm);
    });
  };


  this.getAutoCoderOccupationInfo = function (referenceDatabaseNamePrefix, searchTerm, limit) {
    var deferred = utilityService.callHub(function () {
      return siteHub.server.getAutoCoderOccupationInfo(searchTerm, limit);
    }).then(function (touchData) {
      return touchData;
    });

    return deferred;
  };

  self.lookupOccupation = function (referenceDatabaseNamePrefix, searchTerm, limit) {
    return utilityService.callHub(function () {
      return siteHub.server.lookupOccupation(referenceDatabaseNamePrefix, searchTerm, limit);
    }).then(function (touchData) {
      return touchData;
    });
  };






  this.sendJobPostingsEmail = function (email, savedJobs) {
    return utilityService.callHub(function () {
      return siteHub.server.sendJobPostingsEmail(email, savedJobs);
    });
  };


  this.getExtendedJobPostings = function (touch, selection) {
    return utilityService.callHub(function () {
      return siteHub.server.getExtendedJobPostings(touch, selection);
    });
  };

  //this.generateCsvFile = function (touch, selection) {
  //    return utilityService.callHub(function () {
  //        return siteHub.server.generateCsvFile(touch, selection);
  //    });
  //};

  this.doCommand = function (command, commandData) {
    return utilityService.callHub(function () {
      return siteHub.server.doCommand(command, commandData);
    });
  };


  // ** Participant Group related
  model.participantGroups = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return utilityService.callHub(function () {
        return siteHub.server.searchParticipantGroups(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return utilityService.updateItemsModel(model.participantGroups, itemsData);
      });
    }
  };




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

  this.ensureEventParticipantGroups = function (siteService, eventParticipantsIndex) {
    // TODO: Server-side filtering of participant Group based on eventParticipantsIndex
    return model.participantGroups.search("", "", 0, 999999)
      .then(function (itemsData) {

        //extract unique ParticipantGroupIDs
        var flags = [];
        return $.map(eventParticipantsIndex, function (value, index) {
          var pID = model.eventParticipants.hashMap[value].participantID;
          var pgID = model.participants.hashMap[pID].participantGroupID;
          if (!flags[pgID]) {
            flags[pgID] = true;
            return pgID;
          }
        });
      });
  };


  // returns an Item (not a promise - see ensureXyz() for the promise variant)
  self.demandParticipantGroup = function (itemKey) {
    var modelItems = model.participantGroups;
    return modelItems.hashMap[itemKey] ||
      (
        modelItems.hashMap[itemKey] = { key: itemKey, id: itemKey, code: itemKey, displayTitle: 'loading...' },
        utilityService.delayLoad2(modelItems, itemKey),
        modelItems.hashMap[itemKey]
      );
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
  }





  siteHub.on('updateSettings', function (settingsObject) {
    $rootScope.$apply(onSettingsUpdated(settingsObject));
  }).on('setAuthenticatedEventSession', function (itemsData) {

    $rootScope.$apply(onSetAuthenticatedEventSession(itemsData));

  }).on('updateProjects', function (projectsData) {
    //var notification = onProjectsUpdated(projectsData);
    $rootScope.$apply($rootScope.$broadcast('updateProjects', onProjectsUpdated(projectsData)));

  }).on('updateCompanyLayTitles', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateCompanyLayTitles', itemsData, model.companyLayTitles));


  }).on('updateEvents', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEvents', utilityService.updateItemsModel(model.events, itemsData)));
  }).on('updateEventSessions', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEventSessions', utilityService.updateItemsModel(model.eventSessions, itemsData)));


  }).on('updateParticipantGroups', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateParticipantGroups', utilityService.updateItemsModel(model.participantGroups, itemsData)));

  }).on('updateParticipants', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateParticipants', utilityService.updateItemsModel(model.participants, itemsData)));

  }).on('updateEventParticipants', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateEventParticipants', utilityService.updateItemsModel(model.eventParticipants, itemsData)));





  }).on('updateOccupations', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateOccupations', onOccupationsUpdated(itemsData)));


  }).on('updateCareerProfiles', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateCareerProfiles', utilityService.updateItemsModel(model.careerProfiles, itemsData)));
  }).on('updateCareerSteps', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateCareerSteps', utilityService.updateItemsModel(model.careerSteps, itemsData)));


  }).on('updateClients', function (itemsData) {
    $rootScope.$apply($rootScope.$broadcast('updateClients', utilityService.updateItemsModel(model.clients, itemsData)));
  }).on('touchInit', function (touchData) {
    $rootScope.$apply(onTouchData(touchData));
  });



  function onSetAuthenticatedEventSession(itemsData) {
    // usersData is expected to contain just one user - the authenticated user
    //onUsersUpdated(usersData);
    if (itemsData && itemsData.items) {

      //!! TODO - this users' data might change - need to track that in onUsersUpdated() - but do that after we change the server notification
      var authenticatedItem = itemsData.items[0];
      model.authenticatedIdentity = msIdentity.create('eventSession', authenticatedItem.id, authenticatedItem.name, authenticatedItem.systemRoles, authenticatedItem.profilePhotoUrl);
      model.authenticatedUser = null;

      // head to Kiosk home page
      $state.go('app.event-session-volunteer.check-in', {}, { reload: true });

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


app.filter('demandOccupations', ['demandObjectsFilter', 'siteService', function (demandObjectsFilter, siteService) {
  return function (objects) {
    return demandObjectsFilter(objects, function (object) {
      return siteService.demandOccupation(object.id);
    });
  };
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

