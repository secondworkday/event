
// Define basic Authorization roles here.
// They allow states to be protected based on Role. (Only SystemAdmins can enter system admin states, etc.)

//app.constant('APP_ROLE_TRANSLATION', {
  // Note: This is intended to be defined by the application
//});



app.constant('AUTHORIZATION_ROLES', {
  // Other
  anonymous: "Anonymous",
  authenticated: "Authenticated",
  // SystemRoles
  operationsAdmin: "OperationsAdmin",
  tenantAdmin: "TenantAdmin",
  securityAdmin: "SecurityAdmin",
  systemAdmin: "SystemAdmin"

  // AppRoles

  // Note: This constant can be redefined in site.config.js if additional application roles are required

});


app.run(['$rootScope', '$state', '$stateParams', '$http', '$templateCache', 'utilityService', 'AUTHORIZATION_ROLES', function ($rootScope, $state, $stateParams, $http, $templateCache, utilityService, AUTHORIZATION_ROLES) {

  // this is the generic way to tell an md-tab that user changed to another tab
  // as long as tabs are looking for 'currentTab' and the state definition file
  // includes data for 'selectedTab' Index then this will work
  $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
    $rootScope.currentTab = toState.data.selectedTab;
  });

  $rootScope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {

    // lots 'o debug info
    console.log.bind(console);

    // Standard behavior is to walk up the state tree until we find a safe haven.
    // Note - we also check for {parent}.dashboard as we meander up the tree
    var parentStateName = toState.name;
    while (parentStateName) {
      parentStateName = parentStateName.split('.').slice(0, -1).join('.');
      var parentState = $state.get(parentStateName);
      if (parentState && !parentState.abstract) {
        $state.go(parentStateName, toParams);
        return;
      }
      var dashboardParentStateName = parentStateName + ".dashboard";
      var dashboardParentState = $state.get(dashboardParentStateName);
      if (dashboardParentState && !dashboardParentState.abstract && dashboardParentStateName != toState.name) {
        $state.go(dashboardParentStateName, toParams);
        return;
      }
    }


    // if this ever happens, we want to know about it.
    throw error;
  });

  $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {


    if (toState.redirectTo) {
      event.preventDefault();
      $state.go(toState.redirectTo, toParams);
    }

    //set variables to false
    $rootScope.isAdminState = toState.name.indexOf(".admin.") > -1;
    $rootScope.isSystemState = toState.name.indexOf(".system.") > -1;
    $rootScope.isJobSeekerState = toState.name.indexOf(".js.") > -1;
    //** Authorization
    //   This feels like the perfect place to implement our core Authorization mechanism. Does the user have necessary permission to enter toState? And if not, what to do about it?
    //   Possible alternatives are sending them to a login page, to an alternative page, to an access denied page, or leaving them on their current page
    //   Note: This method fires before resolve methods on the state we're entering. Which begs the question in this method how do we make transition decisions if they also involve
    //   resolving a promise first (like say getting the authenticated user)? Our strategy in that case is to deny the transition, resolve our required information, and then $state.go()
    //   as necessary.
    //   Note: this guy has a transition service that might bring extra power into this method. http://christopherthielen.github.io/ui-router-extras/#/home
    //   Also sounds like it might eventually get rolled into UI-Router directly
    var authenticatedIdenity = utilityService.model.authenticatedIdentity;
    // default to AUTHORIZATION_ROLES.authenticated if no other roles specified
    var stateAllowedRoles = angular.isObject(toState.data) && angular.isArray(toState.data.allowedRoles) ? toState.data.allowedRoles : [AUTHORIZATION_ROLES.authenticated];
    var authenticate = function (stateAllowedRoles, identity) {
      // anyone can enter a state which allows AUTHORIZATION_ROLES.anonymous
      if (stateAllowedRoles.indexOf(AUTHORIZATION_ROLES.anonymous) > -1) {
        return true;
      }

      if (!identity) {
        // deny if we don't have an identity
        return false;
      }

      // anyone authenticated can enter a state which allows (the default) AUTHORIZATION_ROLES.authenticated
      if (stateAllowedRoles.indexOf(AUTHORIZATION_ROLES.authenticated) > -1) {
        return true;
      }

      var identityRoles = identity.roles;
      if (!identityRoles) {
        return false;
      }

      // See if identity has any roles in common with stateAllowedRoles
      var intersection = stateAllowedRoles.filter(function (n) {
        return identityRoles.indexOf(n) != -1;
      });
      if (intersection.length > 0) {
        return true;
      }

      // If we can't find a reason to allow - we've got to deny
      return false;
    };

    if (authenticate(stateAllowedRoles, authenticatedIdenity)) {
      return;
    }

    // stop state change
    event.preventDefault();

    if (!authenticatedIdenity) {

      var toState = toState;
      var toParams = toParams;
      var fromState = fromState;
      var fromParams = fromParams;

      utilityService.whenConnected()
      .then(function () {
        var authenticatedIdenity = utilityService.model.authenticatedIdentity;

        if (!authenticatedIdenity) {
          // log on / sign in...
          $state.go("public.landing", null, { location: 'replace' });
        }
        if (authenticate(stateAllowedRoles, authenticatedIdenity)) {
          // location: 'replace' prevents the browser back button from returning a logged in user to the login page.
          // reload: true forces a reload of controllers/resolves, so our Identity correctly re-resolves
          $state.go(toState, toParams, { location: 'replace', reload: true });
        }
      });
    }
  });

  // SignalR wants you to declare client functions on any Hubs that will be active before starting the connection.
  // We're using these two hubs, so we ensure both have client functions here.
  $.connection.utilityHub.client.fake = function () { };
  $.connection.siteHub.client.fake = function () { };

  // Force-preload our connection related partials - so they're available if we have connection problems and can't fetch them later
  $http.get('/client/states/public/public.html', { cache: $templateCache });
  $http.get('/client/states/public/connection/reconnecting.html', { cache: $templateCache });
  $http.get('/client/states/public/connection/disconnected.html', { cache: $templateCache });

}]);


app.controller('SpaController', function ($scope, $http, $window, $state, SYSTEM_INFO, CONNECTION_STATUS, utilityService) {

  $scope.SYSTEM_INFO = SYSTEM_INFO;

  $scope.jobCommand = function (jobID, command) {
    if (command == 'Cancel') {
      $scope.model.jobs[jobID].shouldCancel = true;
    }

    utilityService.jobCommand(jobID, command);
  };

  $scope.jobShowFilter = function (job, showID) {
    return job.showID == showID;
  };


  $scope.signOut = function () {
    utilityService.signOut()
      .then(function () {
        $state.go('public.landing', {}, { reload: true });
        // $state.go('app.system.users', {}, { reload: true });
      });
  };

  $scope.vespa = function (options) {
    utilityService.vespa(options)
      .then(function (successData) {
        // success
        $mdToast.showSimple("Yessir.");
        console.log("Success: ", successData);
        return successData;
      }, function (failureData) {
        // failure
        $mdToast.showSimple("Nosir.");
        console.log("Failure: ", failureData);
        return failureData;
      }, function (progressData) {
        // progress

        // Remember and reuse one toast - otherwise we flicker as each piece of toast pops up
        var content = 'Progress: ' + progressData.CurrentCount + ' of ' + progressData.TotalCount;
        if (!progressToast) {
          progressToast = $mdToast
            .simple()
            .content(content)
            .hideDelay(0);
          $mdToast.show(progressToast);
        } else {
          $mdToast.updateContent(content);
        }

        console.log("Progress: ", progressData);
        return progressData;
      });
  };



  // init

  utilityService.getJobs();
});





app.controller('HeaderController', function ($scope, $location) {
  $scope.isActive = function (viewLocation) {
    return viewLocation === $location.path();
  };
});


app.service('bootstrapService', ['$rootScope', '$q', 'utilityService', function ($rootScope, $q, utilityService) {

  var bootstrapHub = $.connection.bootstrapHub; // the generated client-side hub proxy

  // BootstrapHub methods

  this.getSqlServerAccountsDatabaseNames = function (serverName, loginName, password) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.getSqlServerAccountsDatabaseNames(serverName, loginName, password);
    });
  };

  this.getS3AccountsDatabaseNames = function (s3AccessKey, s3SecretAccessKey) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.getS3AccountsDatabaseNames(s3AccessKey, s3SecretAccessKey);
    });
  };


  this.remoteDatabase = function (siteConfigData) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.remoteDatabase(siteConfigData);
    });
  };
  this.localDatabase = function (siteConfigData) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.localDatabase(siteConfigData);
    });
  };
  this.createLocalDatabase = function (siteConfigData) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.createLocalDatabase(siteConfigData);
    });
  };
  this.downloadS3Database = function (siteConfigData) {
    return utilityService.callHub(function () {
      return bootstrapHub.server.downloadS3Database(siteConfigData);
    });
  };

  //Init
}]);

app.constant('CONNECTION_EVENT', {
  connectionStarting: 'connection-starting',
  connectionStarted: 'connection-started',
  connectionStopped: 'connection-stopped'
})
.constant('CONNECTION_STATUS', {
  online: 'online',
  disconnected: 'disconnected',
  reconnecting: 'reconnecting'
});

// Authenticated Identity - generally represents a specific User, but could also be a Kiosk, one-time access User, etc.
app.service('msIdentity', function () {
  this.create = function (type, id, displayName, appRoles, systemRoles, profilePhotoUrl) {
    if (!angular.isArray(appRoles)) {
      appRoles = [appRoles];
    }
    if (!angular.isArray(systemRoles)) {
      systemRoles = [systemRoles];
    }
    this.type = type;
    this.id = id;
    this.displayName = displayName;
    this.appRoles = appRoles;
    this.systemRoles = systemRoles;
    this.profilePhotoUrl = profilePhotoUrl;

    //!! do we still require this?
    this.roles = systemRoles.concat(appRoles);


    return this;
  };


  //  model.authenticatedIdentity = msIdentity.createItemUser(type, appRoles, systemRoles, userID, userDisplayName, profilePhotoUrl, itemID);
  this.createUser = function (user) {
    this.appRoles = user.appRoles || [];
    this.systemRoles = user.systemRoles || [];
    this.displayName = user.displayName;
    this.profilePhotoUrl = user.profilePhotoUrl;

    this.userID = user.id;
    this.notGotItSet = user.notGotItSet;

    // (used for permission checks)
    //!! though not really required - could be refactored out
    this.roles = this.systemRoles.concat(this.appRoles);

    return this;
  };





//  model.authenticatedIdentity = msIdentity.createItemUser(type, appRoles, systemRoles, userID, userDisplayName, profilePhotoUrl, itemID);
  this.createItemUser = function (appRoles, systemRoles, displayName, profilePhotoUrl, userID, itemType, itemID) {
    if (!appRoles) {
      appRoles = [];
    } else if (!angular.isArray(appRoles)) {
      appRoles = [appRoles];
    }
    if (!systemRoles) {
      systemRoles = [];
    } else if (!angular.isArray(systemRoles)) {
      systemRoles = [systemRoles];
    }
    this.appRoles = appRoles;
    this.systemRoles = systemRoles;
    this.displayName = displayName;
    this.profilePhotoUrl = profilePhotoUrl;

    this.userID = userID;

    this.itemType = itemType;
    this.itemID = itemID;

    //!! what about this???
    // this.id = id;

    // (used for permission checks)
    //!! though not really required - could be refactored out
    this.roles = systemRoles.concat(appRoles);

    return this;
  };


  this.destroy = function () {
    this.type = null;
    this.id = null;
    this.displayName = null;
    this.profilePhotoUrl = null;
    this.roles = null;
  };
  return this;
});


app.service('msAuthenticated', function (AUTHORIZATION_ROLES) {

  var self = this;

  self.setAuthenticatedGroup = function (group) {
    self.group = group;
  };

  self.setAuthenticatedIdentity = function (msIdentity) {
    self.identity = msIdentity;
  };


  self.isNotGotIt = function (gotItLabel) {
    if (self.identity && self.identity.notGotItSet) {
      return self.identity.notGotItSet.indexOf(gotItLabel) > -1;
    }
    return false;
  };

  self.setGotIt = function (gotItLabel) {
    if (self.identity && self.identity.notGotItSet) {
      var idx = self.identity.notGotItSet.indexOf(gotItLabel);
      if (idx > -1) {
        // toggle off...
        self.identity.notGotItSet.splice(idx, 1);
      }
    }
  };

  self.authorizeRole = function (allowedRoles) {

    allowedRoles = angular.isArray(allowedRoles) ? allowedRoles : [allowedRoles];

    // anyone can enter a state which allows AUTHORIZATION_ROLES.anonymous
    if (allowedRoles.indexOf(AUTHORIZATION_ROLES.anonymous) > -1) {
      return true;
    }

    if (!self.identity) {
      // deny if we don't have an identity
      return false;
    }

    // anyone authenticated can enter a state which allows (the default) AUTHORIZATION_ROLES.authenticated
    if (allowedRoles.indexOf(AUTHORIZATION_ROLES.authenticated) > -1) {
      return true;
    }

    var identityRoles = self.identity.roles;
    if (identityRoles) {
      // See if identityAppRoles has any roles in common with allowedRoles
      var intersection = allowedRoles.filter(function (n) {
        return identityRoles.indexOf(n) > -1;
      });
      if (intersection.length > 0) {
        return true;
      }
    }


    var identitySystemRoles = self.identity.systemRoles;
    if (identitySystemRoles) {
      // See if identitySystemRoles has any roles in common with allowedRoles
      var intersection = allowedRoles.filter(function (n) {
        return identitySystemRoles.indexOf(n) > -1;
      });
      if (intersection.length > 0) {
        return true;
      }
    }

    var identityAppRoles = self.identity.appRoles;
    if (identityAppRoles) {
      // See if identityAppRoles has any roles in common with allowedRoles
      var intersection = allowedRoles.filter(function (n) {
        return identityAppRoles.indexOf(n) > -1;
      });
      if (intersection.length > 0) {
        return true;
      }
    }

    // If we can't find a reason to allow - we've got to deny
    return false;
  };

  return this;
});






app.constant('BADGE_COLORS', [
    '#950c00',
    '#006e3b',
    '#5c0621',
    '#2e4370',
    '#5c0621',
    '#4f4800',
    '#4c6e9e',
    '#8a3e0c'
]);

app.service('utilityService', ['$rootScope', '$q', '$state', '$http', '$window', '$log', 'msIdentity', 'msAuthenticated', 'SYSTEM_INFO', 'TEMPLATE_URL', 'CONNECTION_STATUS', 'CONNECTION_EVENT', 'USER_STATE', 'BADGE_COLORS', function ($rootScope, $q, $state, $http, $window, $log, msIdentity, msAuthenticated, SYSTEM_INFO, TEMPLATE_URL, CONNECTION_STATUS, CONNECTION_EVENT, USER_STATE, BADGE_COLORS) {

  var self = this;

  // the generated client-side hub proxy - only callable when promise held by hubReady is resolved. Use callHub() calling pattern to ensure this.
  var utilityHub = $.connection.utilityHub;
  // holds promised returned when starting our hub connection or null if we haven't started the connection yet
  var hubReady;
  // holds the ui-router state the application was in when the connection with the server was lost
  var connectionLostState;


  self.buildSearchExpression = function (/*filters*/) {
    var filterHandler = function (searchTerms, filter) {
      if (typeof filter === 'string') {
        // direct search component like 'pete'
        searchTerms.push(filter);
      }
      else if (angular.isObject(filter)) {
        if (filter.serverTerm) {
          //!! consider what type of serverTerm we've got. If it's a string add it, else if it's an object, add its properties (as we do below)
          searchTerms.push(filter.serverTerm);
        }
        else {
          // add the properties of this object as serverTerms (presumably there's no associated clientFilter as for immutable data)

          angular.forEach(filter, function (value, key) {
            if (key && value) {
              searchTerms.push('$' + key + ':' + value);
            }
          });

          // $log.debug('object type?', filter);
        }
      }
    };

    var filterTerms = buildFilterTerms(arguments, filterHandler);
    var searchExpression = filterTerms.join(' ');
    return searchExpression;
  };

  self.buildClientFilter = function (/*filters*/) {
    var clientFilterHandler = function (clientFilterTerms, filter) {
      if (angular.isObject(filter)) {
        if (filter.clientFunction) {
          clientFilterTerms.push(filter.clientFunction);
        }
        else {
          $log.debug('object type?', filter);
        }
      }
    };

    var filterTerms = buildFilterTerms(arguments, clientFilterHandler);
    return filterTerms;
  };



  function buildFilterTerms(hierarchy, filterHandler) {

    var filterTermHandler = function (searchTerms, filter) {
      if (typeof filter === 'undefined') {
        // nothing to do
      }
      else if (typeof filter === 'string' || typeof filter === 'object') {
        filterHandler(searchTerms, filter);
      }
      else {
        $log.debug('type?', filter);
      }
    };

    var filterTerms = [];
    angular.forEach(hierarchy, function (filter) {
      if (angular.isArray(filter)) {
        angular.forEach(filter, function (filter) {
          filterTermHandler(filterTerms, filter);
        });
      } else {
        filterTermHandler(filterTerms, filter);
      }
    });
    return filterTerms;
  }


  self.toggleList = function (list, item) {
    var idx = list.indexOf(item);
    if (idx > -1) {
      // toggle off...
      list.splice(idx, 1);
    }
    else {
      // toggle on...
      list.push(item);
    }
  };

  self.listContains = function (list, item) {
    return list.indexOf(item) > -1;
  };





  // Some handy compare functions

    function compareByID(left, right) {
      return left.id - right.id;
    }
    this.compareByID = compareByID;

    function compareByIDDescending(left, right) {
      return compareByID(right, left);
    }
    this.compareByIDDescending = compareByIDDescending;




    self.filterByFeed = function (feedTag) {
      var feedTag = feedTag;
      return function (item) {
        return item.feeds.indexOf(feedTag) != -1;
      };
    };

    self.filterByList = function (listTag) {
      var listTag = listTag;
      return function (item) {
        return item.lists.indexOf(listTag) != -1;
      };
    };


  // ** Helper to create compare function for ms-search-view

    self.compareByProperties = function (propertyName /*, propertyName2, propertyName3*/) {
      var propertyNames = arguments;
      if (propertyNames.length > 1) {
        return function (left, right) {
          var i = 0, result = 0;
          /* try getting a different result from 0 (equal)
           * as long as we have extra properties to compare
           */
          while (result === 0 && i < propertyNames.length) {
            result = self.compareByProperties(propertyNames[i])(left, right);
            i++;
          }
          return result;
        };
      }
      var sortOrder = 1;
      if (propertyName[0] === "-") {
        sortOrder = -1;
        propertyName = propertyName.substr(1);
      }
      return function (left, right) {

        if (!(propertyName in left)) {
          $log.error('Property ' + propertyName + ' not found', left);
        }
        if (!(propertyName in right)) {
          $log.error('Property ' + propertyName + ' not found', right);
        }

        var result = (left[propertyName] < right[propertyName]) ? -1 : (left[propertyName] > right[propertyName]) ? 1 : 0;
        return result * sortOrder;
      }
    };

    self.filterByPropertyValue = function (propertyName, value) {
      var value = value;
      return function (item) {
        return item[propertyName] === value;
      };
    };

    self.filterByPropertyHasValue = function (propertyName) {
      var propertyName = propertyName;
      if (propertyName[0] === "!") {
        propertyName = propertyName.substr(1);
        return function (item) {
          return !item[propertyName];
        };
      }
      return function (item) {
        return item[propertyName];
      };
    };







  //!! Think these are all depricated - in favor of compareByProperties() above!!!!!


    self.compareByPropertyThenByID = function (propertyName) {
      var propertyName = propertyName;
      return function (left, right) {
        // first try to compare by the property
        return (left[propertyName] || 0) - (right[propertyName] || 0)
          // if they're equal, fall back on comparing the IDs
          || left.id - right.id;
      };
    };
    self.compareByPropertyThenByIDDescending = function (propertyName) {
      var propertyName = propertyName;
      // swap left/right to achieve descending
      return function (right, left) {
        // first try to compare by the property
        return (left[propertyName] || 0) - (right[propertyName] || 0)
          // if they're equal, fall back on comparing the IDs
          || left.id - right.id;
      };
    };

    self.localeCompareByPropertyThenByID = function (propertyName) {
      var propertyName = propertyName;
      return function (left, right) {
        // first try to compare by the (text) property
        return (left[propertyName] || "").localeCompare(right[propertyName] || "")
          // if they're equal, fall back on comparing the IDs
          || left.id - right.id;
      };
    };
    self.localeCompareByPropertyThenByIDDescending = function (propertyName) {
      var propertyName = propertyName;
      // swap left/right to achieve descending
      return function (right, left) {
        // first try to compare by the (text) property
        return (left[propertyName] || "").localeCompare(right[propertyName] || "")
          // if they're equal, fall back on comparing the IDs
          || left.id - right.id;
      };
    };

    var model = {

        // SignalR provide connection states - see http://www.asp.net/signalr/overview/guide-to-the-api/handling-connection-lifetime-events
        serverConnectionStatus:  CONNECTION_STATUS.disconnected,
        serverConnectionSlow: false, // { true | false }

        authenticatedIdentity: null,
        authenticatedUser: null,

        jobsIndex: [],
        jobs: {},

        databaseLog: {},
        databaseLogIndex: [],
        databaseLogTotalCount: null
    };

    var slowTimer;

    this.calculateElapsedSeconds = function (dateNow, dateStart) {
        var dateDeltaMilliseconds = dateNow.getTime() - dateStart.getTime();
        return Math.max(0, dateDeltaMilliseconds / 1000);
    };

    this.httpGet = function (url, config) {
        return callHub(function () {
            url = buildUrl(url, config.params);
            return utilityHub.server.httpGet(url);
        }).then(function (responseData) {

            if (config.transformResponse) {
                responseData = config.transformResponse(responseData);
            }

            return responseData;
        });
    };

    function buildUrl(url, params) {
        if (!params) return url;
        var parts = [];
        forEachSorted(params, function (value, key) {
            if (value === null || isUndefined(value)) return;
            if (!angular.isArray(value)) value = [value];

            angular.forEach(value, function (v) {
                if (angular.isObject(v)) {
                    v = angular.toJson(v);
                }
                parts.push(encodeUriQuery(key) + '=' +
                           encodeUriQuery(v));
            });
        });
        return url + ((url.indexOf('?') == -1) ? '?' : '&') + parts.join('&');
    }

    function isUndefined(value) { return typeof value === 'undefined'; }

    function sortedKeys(obj) {
        var keys = [];
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                keys.push(key);
            }
        }
        return keys.sort();
    }

    function forEachSorted(obj, iterator, context) {
        var keys = sortedKeys(obj);
        for (var i = 0; i < keys.length; i++) {
            iterator.call(context, obj[keys[i]], keys[i]);
        }
        return keys;
    }

    function encodeUriQuery(val, pctEncodeSpaces) {
        return encodeURIComponent(val).
                   replace(/%40/gi, '@').
                   replace(/%3A/gi, ':').
                   replace(/%24/g, '$').
                   replace(/%2C/gi, ',').
                   replace(/%20/g, (pctEncodeSpaces ? '%20' : '+'));
    }
    function encodeUriQuery(val, pctEncodeSpaces) {
        return encodeURIComponent(val).
                   replace(/%40/gi, '@').
                   replace(/%3A/gi, ':').
                   replace(/%24/g, '$').
                   replace(/%2C/gi, ',').
                   replace(/%20/g, (pctEncodeSpaces ? '%20' : '+'));
    }

    this.getModel = function () {
        return model;
    };
    self.model = model;


    // UtilityHub methods


  //** VESPA - Validate, Error, Success, Progress, Action

  self.vespa = function () {
    return callHub(function () {
      return utilityHub.server.vespa();
    });
  };

  self.vespa = function (parameters) {
    return callHub(function () {
      return utilityHub.server.vespa(parameters);
    });
  };



  self.vespaRunTask = function (parameters) {
    return callHub(function () {
      return utilityHub.server.vespaRunTask(parameters);
    });
  };


  self.vespaStartTask = function (parameters) {
    return callHub(function () {
      return utilityHub.server.vespaStartTask(parameters);
    });
  };

  self.vespaTrackTask = function (taskItem) {
    return callHub(function () {
      return utilityHub.server.vespaTrackTask(taskItem.taskID);
    });
  };

  self.vespaCancelTask = function (taskItem) {
    return callHub(function () {
      return utilityHub.server.vespaCancelTask(taskItem.taskID);
    });
  };



  self.waitTask = function (taskItem) {
    return callHub(function () {
      return utilityHub.server.waitTask(taskItem.taskID);
    });
  };

  self.cancelTask = function (taskItem) {
    return callHub(function () {
      return utilityHub.server.cancelTask(taskItem.taskID);
    });
  };







    function joinGroup(groupName) {
        return callHub(function () {
            return utilityHub.server.joinGroup(groupName);
        });
    };

    function leaveGroup(groupName) {
        return callHub(function () {
            return utilityHub.server.leaveGroup(groupName);
        });
    };


    this.getJobs = function () {

      return callHub(function () {
        return utilityHub.server.getJobs();
      }).then(function (jobsData) {
        onJobsDataUpdated(jobsData);
        return jobsData;
      });
    };

    this.jobCommand = function (jobID, command) {
        return callHub(function () {
            return utilityHub.server.jobCommand(jobID, command);
        });
    };

    function onJobsDataUpdated(jobsData) {
        purgeItems(jobsData.deletedIDs, model.jobs, model.jobsIndex);
        cacheItems(jobsData.items, model.jobs, model.jobsIndex);
        model.jobsTotalCount = jobsData.totalCount;
    };


  this.sendSmsMessage = function (phoneNumber, message) {
      return callHub(function () {
          return utilityHub.server.sendSmsMessage(phoneNumber, message);
      });
  };



  self.raiseDiagnosticEvent = function (event, data) {
    return callHub(function () {
      return utilityHub.server.raiseDiagnosticEvent(event, data);
    });
  };


  self.generateCriticalEventLogItem = function (description) {
    return callHub(function () {
      return utilityHub.server.generateCriticalEventLogItem(description);
    });
  };






  self.getReportTemplateInfo = function () {
    return callHub(function () {
      return utilityHub.server.getReportTemplateInfo();
    });
  };

  self.removeReportTemplateOverrideFile = function (reportTemplate) {
    return callHub(function () {
      return utilityHub.server.removeReportTemplateOverrideFile(reportTemplate);
    });
  };






    //** Authentication Related

    this.isInRole = function (role) {
        if (model.authenticatedIdentity) {
            // role == authenticatedIdentity.type == "hospital" --> kiosk mode
            if (model.authenticatedIdentity.type == role) {
                return true;
            }
            return ($.inArray(role, model.authenticatedIdentity.roles) > -1);
        }
        return false;
    };



  //** Task related

  model.myTasks = {
    hashMap: {},
    index: [],

    // tracks in memory the presumably few myTasks that are active
    activeIndexer: {
      index: [],
      sort: self.compareByProperties('name', 'id'),
      filter: function (item) {
        return item.state == 'Active';
      }
    }

    // consider blockedIndex, other states
    // don't think we need completedIndex - leave that up to the view controller. otherwise decay them out

    //!! todo add in stats
    //!! todo add in delay load
  };

  function onMyTasksUpdated(itemsData) {

    // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)
    var modelItems = model.myTasks;
    self.purgeItems(itemsData.deletedIDs, modelItems.hashMap, modelItems.index, modelItems.activeIndexer.index);
    var newItemIDs = cacheSortedItems(itemsData.items, modelItems.hashMap, modelItems.index, modelItems.activeIndexer);

    //!! todo remember to update our STATs!

    var notification = {
      hashMap: modelItems.hashMap,
      ids: $.map(itemsData.items, function (item) {
        return item.id;
      }),
      newIDs: newItemIDs,
      deletedIDs: itemsData.deletedIDs,
      totalCount: itemsData.totalCount,
      resolvedIDs: []
    };

    return notification;
  };


/*
    this.getActiveAndBlockedCases = function () {
      this.searchCases('$Submitted $Rejected', '', 0, 999999);
    };

    this.getClosedCases = function (startIndex, rowCount) {
      return this.searchClosedCases('', startIndex, rowCount);
    };
*/

  self.searchMyTasks = function (searchExpression, sortExpression, startIndex, rowCount) {
    return self.callHub(function () {
      return utilityHub.server.searchMyTasks(searchExpression, sortExpression, startIndex, rowCount);
    }).then(function (itemsData) {
      return onMyTasksUpdated(itemsData);
    });
  };

  self.createMyTask = function (myTaskName) {

    var onMyTasksUpdatedNotification = {
      deletedIDs: [],
      totalCount: 1,
      items: [
        { id: new Date().getTime(), name: myTaskName, state: 'Active' }
      ]
    };

    onMyTasksUpdated(onMyTasksUpdatedNotification);
    return;

    //!! for later
    return self.callHub(function () {
      return utilityHub.server.createMyTask(myTaskName);
    });
  };

  self.editMyTask = function (itemID, editItem) {

    model.myTasks.hashMap[itemID].name = editItem.name;
    return;

    //!! for later
    return self.callHub(function () {
      return utilityHub.server.editMyTask(itemID, editItem);
    });
  };

  self.closeMyTask = function (item) {

    item.state = "Closed";

    var onMyTasksUpdatedNotification = {
      deletedIDs: [item.id],
      totalCount: 0,
      items: []
    };

    onMyTasksUpdated(onMyTasksUpdatedNotification);
    return;

    //!! for later
    return self.callHub(function () {
      return utilityHub.server.closeMyTask(item.id);
    });
  };







  //** Tenant Related

  model.tenantGroups = {
    hashMap: {},
    index: [],

    groupsIndexer: {
      index: [],
      sort: self.compareByProperties('name', 'id'),
      filter: function (item) {
        return item.parentID;
      }
    },

    tenantsIndexer: {
      index: [],
      sort: self.compareByProperties('name', 'id'),
      filter: function (item) {
        return !item.parentID;
      }
    },

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return callHub(function () {
        return utilityHub.server.searchTenantGroups(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return onTenantGroupsUpdated(model.tenantGroups, itemsData);
      });
    }
  };

  // (actually supports both tenants (top level) and groups (children of tenants)
  function onTenantGroupsUpdated(itemsModel, itemsData) {
    // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)

    self.purgeItems(itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index, itemsModel.groupsIndexer.index, itemsModel.tenantsIndexer.index);
    var newItemIDs = cacheSortedItems(itemsData.items, itemsModel.hashMap, itemsModel.index, itemsModel.tenantsIndexer, itemsModel.groupsIndexer);

    if (model.authenticatedGroup) {
      // refresh our authenticated group (might be a no-op)
      model.authenticatedGroup = itemsModel.hashMap[model.authenticatedGroup.id];
    }

    var notification = {
      hashMap: itemsModel.hashMap,
      ids: $.map(itemsData.items, function (item) {
        return item.id;
      }),
      newIDs: newItemIDs,
      deletedIDs: itemsData.deletedIDs,
      totalCount: itemsData.totalCount,
      resolvedIDs: []
    };

    return notification;
  };

  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
  self.ensureTenantGroup = function (itemKey) {
    var modelItems = model.tenantGroups;
    var item = modelItems.hashMap[itemKey];
    if (!item) {
      return modelItems.search("%" + itemKey, "", 0, 1)
      .then(function (ignoredNotificationData) {
        return modelItems.hashMap[itemKey];
      });
    }

    return $q.when(item);
  };

  // returns an Item (not a promise - see ensureXyz() for the promise variant)
  self.demandTenantGroup = function (itemKey) {
    var modelItems = model.tenantGroups;
    return modelItems.hashMap[itemKey] ||
      (
        modelItems.hashMap[itemKey] = { id: itemKey, code: itemKey, displayTitle: 'loading...' },
        self.delayLoad2(modelItems, itemKey),
        modelItems.hashMap[itemKey]
      );
  };



    this.getTenants = function () {
      return model.tenantGroups.search("^", "", 0, 999999);
    };

    this.getMyGroups = function () {
      var authenticatedGroup = model.authenticatedGroup;
      return this.getGroups(authenticatedGroup);
    };

    this.getGroups = function (parentGroup) {
      return model.tenantGroups.search("^" + parentGroup.id, "", 0, 999999);
    };


    this.createTenant = function (tenantName, data) {
        return callHub(function () {
            return utilityHub.server.createTenant(tenantName, data);
        });
    };

    this.editTenant = function (tenantID, data) {
        return callHub(function () {
            return utilityHub.server.editTenant(tenantID, data);
        });
    };

    // SystemAdmin only
    this.modifyTenantGroupAccountOption = function (tenantGroup, optionName, isAssigned) {
      return callHub(function () {
        return utilityHub.server.modifyTenantGroupAccountOption(tenantGroup.id, optionName, isAssigned);
      });
    };
    // SystemAdmin & TenantAdmin
    this.modifyTenantGroupSettingOption = function (tenantGroup, optionName, isAssigned) {
      return callHub(function () {
        return utilityHub.server.modifyTenantGroupSettingOption(tenantGroup.id, optionName, isAssigned);
      });
    };



    this.modifyMyFavoriteTenantGroup = function (tenantGroup, isFavorite) {
      return callHub(function () {
        return utilityHub.server.modifyMyFavoriteTenantGroup(tenantGroup.id, isFavorite);
      });
    };

    this.deleteTenant = function (tenantID) {
        return callHub(function () {
            return utilityHub.server.deleteTenant(tenantID);
        });
    };


    this.createSubTenant = function (subTenantName, data) {
        return callHub(function () {
            return utilityHub.server.createSubTenant(subTenantName, data);
        });
    };

    this.editSubTenant = function (subTenantID, data) {
        return callHub(function () {
            return utilityHub.server.editSubTenant(tenantID, data);
        });
    };

    this.deleteSubTenant = function (subTenantID) {
        return callHub(function () {
            return utilityHub.server.deleteSubTenant(tenantID);
        });
    };







  //** User Related

    model.users = {
      hashMap: {},
      index: [],

      activeUsersIndexer: {
        index: [],
        sort: self.localeCompareByPropertyThenByID('displayName'),
        filter: function (item) {
          return item.state === USER_STATE.active;
        }
      },
      disabledUsersIndexer: {
        index: [],
        sort: self.localeCompareByPropertyThenByID('displayName'),
        filter: function (item) {
          return item.state === USER_STATE.disabled;
        }
      },

      search: function (searchExpression, sortExpression, startIndex, rowCount) {
        return callHub(function () {
          return utilityHub.server.searchUsers(searchExpression, sortExpression, startIndex, rowCount);
        }).then(function (usersData) {
          //!! one of the two...
          return onUsersUpdated(model.users, usersData);
          //!! return utilityService.updateItemsModel(model.occupations, itemsData);
        });
      }
    };

  // We have a customized version of this (instead of self.updateItemsModel()) as we want to manage some custom indexers
    function onUsersUpdated(itemsModel, itemsData) {
      // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)

      self.purgeItems(itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index, itemsModel.activeUsersIndexer.index, itemsModel.disabledUsersIndexer.index);
      //!! do we need a self?
      var newItemIDs = cacheSortedItems(itemsData.items, itemsModel.hashMap, itemsModel.index, itemsModel.activeUsersIndexer, itemsModel.disabledUsersIndexer);
      //!! check the sorted bit, and that we're doing the indexers right


      //!! var newItemIDs = cacheSortedItems(itemsData.items, model.users, model.usersIndex, model.activeUsersIndexer, model.disabledUsersIndexer);

      if (model.authenticatedUser) {
        // refresh our authenticated user (may or may not include any updates)
        model.authenticatedUser = itemsModel.hashMap[model.authenticatedUser.id];
      }

      var notification = {
        hashMap: itemsModel.hashMap,
        ids: $.map(itemsData.items, function (item) {
          return item.id;
        }),
        newIDs: newItemIDs,
        deletedIDs: itemsData.deletedIDs,
        totalCount: itemsData.totalCount,
        resolvedIDs: []
      };

      //!! what about this?
      // We maintain an "activeIssuesIndex" - which tracks the relatively few Issues that are open. Clients can rely on this if they choose.
      //!!utilityService.indexMerge(model.activeIssuesIndex, notification.ids, model.issues, model.activeIssuesIndexFilter, model.activeIssuesIndexSort);

      return notification;
    };





  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
    self.ensureUser = function (itemKey) {
      var modelItems = model.users;
      var item = modelItems.hashMap[itemKey];
      if (!item) {
        return modelItems.search("%" + itemKey, "", 0, 1)
        .then(function (ignoredNotificationData) {
          return modelItems.hashMap[itemKey];
        });
      }

      return $q.when(item);
    };


  // returns an Item (not a promise - see ensureXyz() for the promise variant)
    self.demandUser = function (itemKey) {
      var modelItems = model.users;
      return modelItems.hashMap[itemKey] ||
        (
          modelItems.hashMap[itemKey] = { id: itemKey, code: itemKey, displayTitle: 'loading...' },
          self.delayLoad2(modelItems, itemKey),
          modelItems.hashMap[itemKey]
        );
    };


    self.setUserGotIt = function (identity, gotItLabel) {
      return callHub(function () {
        return utilityHub.server.setUserGotIt(identity.userID, gotItLabel);
      });
    };




















  //** AuthToken Related
  //  (Note: AuthTokens are stored at page $scope, not in the model

  this.findAuthToken = function (authCode) {
    return callHub(function () {
      return utilityHub.server.findAuthToken(authCode);
    });
  };


  // Must be authenticated
  this.getAuthTokens = function (searchExpression) {
    return callHub(function () {
      return utilityHub.server.searchAuthTokens(searchExpression, "", 0, 999999);
    });
  };

  // Must be authenticated
  this.searchAuthTokens = function (searchExpression, sortExpression, startIndex, rowCount) {
    return callHub(function () {
      return utilityHub.server.searchAuthTokens(searchExpression, sortExpression, startIndex, rowCount);
    });
  };

  this.revokeAuthToken = function (authTokenID) {
    return callHub(function () {
      return utilityHub.server.revokeAuthToken(authTokenID);
    });
  };

  this.resendAuthToken = function (authTokenID) {
    return callHub(function () {
      return utilityHub.server.resendAuthToken(authTokenID);
    });
  };

  this.reissueAuthToken = function (authTokenID) {
    return callHub(function () {
      return utilityHub.server.reissueAuthToken(authTokenID);
    });
  };
















  //** Conversation related

  model.conversations = {};
  // sorted by id
  model.conversationsIndex = [];

  model.myConversation = null;

  function onConversationsUpdated(itemsData, isExternalEvent) {
    // Update our main cache (model.conversations) and cacheIndex (model.conversationsIndex)
    purgeItems(itemsData.deletedIDs, model.conversations, model.conversationsIndex);
    var newItemIDs = cacheItems(itemsData.items, model.conversations, model.conversationsIndex);


    //!!
    if (!model.myConversation && model.authenticatedUser && newItemIDs && newItemIDs.length) {
      for (var i = 0, len = newItemIDs.length; i < len; i++) {
        var conversation = model.conversations[newItemIDs[i]];
        if (conversation.customerUserID === model.authenticatedUser.id) {
          model.myConversation = conversation;
          console.log('Loop is going to break.');
          break;
        }
        console.log('Loop will continue.');
      }
    }

    var notification = {
      hashMap: model.conversations,
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
      $rootScope.$broadcast('updateConversations', notification);
    }

    return notification;
  };

  function searchConversations(searchExpression, sortExpression, startIndex, rowCount) {
    return callHub(function () {
      return utilityHub.server.searchConversations(searchExpression, sortExpression, startIndex, rowCount);
    }).then(function (itemsData) {
      return onConversationsUpdated(itemsData);
    });
  };
  this.searchConversations = searchConversations;

  this.getClosedConversations = function (startIndex, rowCount) {
    return this.searchClosedConversations('', startIndex, rowCount);
  };

  this.searchClosedConversations = function (searchExpression, startIndex, rowCount) {
    //var searchExpression = searchExpression ? searchExpression + " $Resolved" : "$Resolved";
    // note - if we allow caller to pass in a sort parameter, we can't rely on results in closedCasesIndex to reflect closedSortExpression order
    var closedSortExpression = 'item.stateTimestamp DESC';
    return searchConversations(searchExpression, closedSortExpression, startIndex, rowCount);
  };

  this.ensureConversation = function (conversationID) {
    var conversation = model.conversations[conversationID];
    if (!conversation) {
      return searchConversations("%" + conversationID, "", 0, 999999)
      .then(function (conversationsData) {
        return model.conversations[conversationID];
      });
    }

    return $q.when(conversation);
  };

  this.ensureCustomerConversation = function () {

    // If we're not authenticated we can't have a Customer Conversation
    var authenticatedUser = model.authenticatedUser;
    if (!authenticatedUser) {
      return;
    }

    // Just return it if we've already got our customer conversation
    if (model.myConversation) {
      return model.myConversation;
    }

    // We don't actually know our conversationID - as it's determined by the server.
    // We therefore pass along our UserID and let the server figure it out
    return searchConversations("%user:" + authenticatedUser.id, "", 0, 999999)
    .then(function (conversationsData) {
      return;
      //!! return model.conversations[conversationID];
    });
  };


  this.createRandomConversation = function () {
    return callHub(function () {
      return utilityHub.server.createRandomConversation();
    });
  };

  this.setConversationState = function (conversation, state, data) {
    return callHub(function () {
      return utilityHub.server.setConversationState(conversation.id, state, data);
    });
  };

  this.addConversationMessage = function (conversation, message) {
    return callHub(function () {
      return utilityHub.server.addConversationMessage(conversation.id, message);
    });
  };

  this.addCustomerConversationMessage = function (message) {
    return callHub(function () {
      return utilityHub.server.addCustomerConversationMessage(message);
    });
  };








    //!! for testing
    this.generateRandomContact = function () {
        return callHub(function () {
            return utilityHub.server.generateRandomContact();
        });
    };


    this.createDemoUser = function (appRole) {
      return callHub(function () {
        return utilityHub.server.createDemoUser(appRole);
      });
    };


    //!! for testing
    this.createRandomUser = function (appRole) {
      return callHub(function () {
        return utilityHub.server.createRandomUser(appRole);
      });
    };

    this.torqCreateDemoUser = function (appRole) {
      return callHub(function () {
        return utilityHub.server.torqCreateDemoUser(appRole);
      });
    };

    this.getTenantUsers = function (tenant, roles) {

      var searchExpression = "$tid:" + tenant.id + " " + "$role:" + roles;
      return model.users.search(searchExpression, '', 0, 999999);
    };


    this.getUsers = function () {
      return model.users.search('', '', 0, 999999);
    };








    this.createUser = function (user) {
        return callHub(function () {
            return utilityHub.server.createUser(user);
        });
    };

    this.addUser = function (newUser) {
        return callHub(function () {
            return utilityHub.server.createUser(newUser);
        });
    };

    this.setUserTags = function (user, tagNames) {
      return callHub(function () {
        return utilityHub.server.setUserTags(user.id, tagNames);
      });
    };

    this.editUser = function (UserID, editUser) {
        return callHub(function () {
            return utilityHub.server.editUser(UserID, editUser);
        });
    };

    this.updateUser = function (userID, data) {
      return callHub(function () {
        return utilityHub.server.updateUser(userID, data);
      });
    };


    this.modifyUserSystemRole = function (user, systemRole, isAssigned) {
        return callHub(function () {
            return utilityHub.server.modifyUserSystemRole(user.id, systemRole, isAssigned);
        });
    };


    this.deleteUser = function (User) {
        return callHub(function () {
            return utilityHub.server.deleteUser(User.id);
        });
    };

    this.updateUsers = function (usersData) {
        $rootScope.$apply(onUsersUpdated(usersData));
    }





    this.signIn = function (credentials) {
      // Post to server and try to login...
      stopConnection();
      return $http({ method: 'POST', url: '/signin/sitelogin.ashx', data: credentials })
        .then(function () {
          // success
          // chain to startConnection() since it is async too
          return startConnection();
        }, function (failureData) {
          // failure
          // chain to startConnection(), but then return the signin failure reason
          return startConnection()
          .then(function () {
            // success
            return $q.reject(failureData.data);
          });
        });
    };

    this.signOut = function () {
        stopConnection();
        return $http({ method: 'GET', url: '/signin/signout.ashx' })
          .then(function () {
            // success
            // chain to startConnection() since it is async too
            return startConnection();
          }, function (failureData) {
            // failure
            // chain to startConnection(), but then return the signin failure reason
            return startConnection()
            .then(function () {
              return failureData;
            });
          });
    };



  self.impersonateUser = function (user) {
    return this.impersonate({ uid: user.id });
  };

  self.impersonateSystem = function (tenant) {
    return this.impersonate({ tid: tenant.id });
  };

  self.impersonate = function (params) {
    // Post to server and try to login...

    // null our authenticatedUser, which indicates this hub stoppage is expected
    var authenticatedUser = model.authenticatedUser;
    //model.authenticatedIdentity = null;
    //model.authenticatedUser = null;
    stopConnection();

    return $http({ method: 'POST', url: '/system/impersonate.ashx', params: params })
        .success(function (data, status, headers, config) {
          return startConnection();
        }).error(function (data, status, headers, config) {
          // called asynchronously if an error occurs
          // or server returns response with an error status.
        });
  };


    this.resetPassword = function (authCode, newPassword) {
        stopConnection();

        var credentials = { authCode: authCode, newPassword: newPassword };
        return $http({ method: 'POST', url: '/signin/reset-password.ashx', data: credentials });
        // Note we don't restart the connection ...
    };



    this.download = function (query) {
        //!! consider using a hub call to generate the document as a long running, cancellable task, then just do the href thing to fetch it once the bytes are available
        $window.location.href = "/Download.ashx?" + $.param(query);
    };


    this.getSqlAccountsDatabaseNames = function () {
        return callHub(function () {
            return utilityHub.server.getSqlAccountsDatabaseNames();
        });
    };
    this.getSqlReferenceDatabaseNames = function () {
        return callHub(function () {
            return utilityHub.server.getSqlReferenceDatabaseNames();
        });
    };
    this.getS3AccountsDatabaseNames = function () {
        return callHub(function () {
            return utilityHub.server.getS3AccountsDatabaseNames();
        });
    };
    this.getS3ReferenceDatabaseNames = function () {
        return callHub(function () {
            return utilityHub.server.getS3ReferenceDatabaseNames();
        });
    };

    this.backupAccountsDatabase = function (databaseName) {
        return callHub(function () {
            return utilityHub.server.backupAccountsDatabase(databaseName);
        });
    };
    this.backupReferenceDatabase = function (databaseName) {
        return callHub(function () {
            return utilityHub.server.backupReferenceDatabase(databaseName);
        });
    };
    this.restoreAccountsDatabase = function (databaseName) {
        return callHub(function () {
            return utilityHub.server.restoreAccountsDatabase(databaseName);
        });
    };
    this.restoreReferenceDatabase = function (databaseName) {
        return callHub(function () {
            return utilityHub.server.restoreReferenceDatabase(databaseName);
        });
    };








  //** EventLog related

  model.eventLog = {
    hashMap: {},
    index: [],

    search: function (searchExpression, sortExpression, startIndex, rowCount) {
      return callHub(function () {
        return utilityHub.server.searchEventLog(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (itemsData) {
        return onEventLogUpdated(model.eventLog, itemsData);
      });
    }
  };

  function onEventLogUpdated(itemsModel, itemsData) {
    // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)

    self.purgeItems(itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index);
    var newItemIDs = cacheItems(itemsData.items, itemsModel.hashMap, itemsModel.index, self.compareByProperties('-firstOccurenceTimestamp', '-id'));

    var notification = {
      hashMap: itemsModel.hashMap,
      ids: $.map(itemsData.items, function (item) {
        return item.id;
      }),
      newIDs: newItemIDs,
      deletedIDs: itemsData.deletedIDs,
      totalCount: itemsData.totalCount,
      resolvedIDs: []
    };

    return notification;
  };

  // reference count of the number of callers that require server notifications of changes
  var trackEventLogCount = 0;
  this.trackEventLog = function () {
    if (++trackEventLogCount == 1) {
      joinGroup("eventLog");
    }
  };
  this.untrackEventLog = function () {
    if (--trackEventLogCount == 0) {
      leaveGroup("eventLog");
    }
  };

  this.getEventLog = function (startIndex, rowCount) {
    return model.eventLog.search('', '', startIndex, rowCount);
  };




    // reference count of the number of callers that require server notifications of changes
    var trackDatabaseLogCount = 0;
    this.trackDatabaseLog = function () {
        if (++trackDatabaseLogCount == 1) {
            joinGroup("databaseLog");
        }
    };
    this.untrackDatabaseLog = function () {
        if (--trackDatabaseLogCount == 0) {
            leaveGroup("databaseLog");
        }
    };

    this.getDatabaseLog = function (startIndex, rowCount) {
        return searchDatabaseLog('', startIndex, rowCount);
    };

    this.searchDatabaseLog = function (searchExpression, sortExpression, startIndex, rowCount) {
      return callHub(function () {
        return utilityHub.server.searchDatabaseLog(searchExpression, sortExpression, startIndex, rowCount);
      }).then(function (logData) {
        var notification = onDatabaseLogUpdated(logData);
        if (!searchExpression) {
          model.databaseLogTotalCount = notification.totalCount;
        }
        return notification;
      });
    };







  //** ActivityLog related

    model.activityLog = {
      hashMap: {},
      index: [],

      search: function (searchExpression, sortExpression, startIndex, rowCount) {
        return callHub(function () {
          return utilityHub.server.searchActivityLog(searchExpression, sortExpression, startIndex, rowCount);
        }).then(function (itemsData) {
          return onActivityLogUpdated(model.activityLog, itemsData);
        });
      },

      getSet: function (itemID) {
        return callHub(function () {
          return utilityHub.server.getActivityLogSet(itemID);
        });
      }

    };

    function onActivityLogUpdated(itemsModel, itemsData) {
      // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)

      self.purgeItems(itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index);
      var newItemIDs = cacheItems(itemsData.items, itemsModel.hashMap, itemsModel.index, self.compareByProperties('-id'));

      var notification = {
        hashMap: itemsModel.hashMap,
        ids: $.map(itemsData.items, function (item) {
          return item.id;
        }),
        newIDs: newItemIDs,
        deletedIDs: itemsData.deletedIDs,
        totalCount: itemsData.totalCount,
        resolvedIDs: []
      };

      return notification;
    };

  // reference count of the number of callers that require server notifications of changes
    var trackActivityLogCount = 0;
    this.trackActivityLog = function () {
      if (++trackActivityLogCount == 1) {
        joinGroup("activityLog");
      }
    };
    this.untrackActivityLog = function () {
      if (--trackActivityLogCount == 0) {
        leaveGroup("activityLog");
      }
    };

    this.getActivityLog = function (startIndex, rowCount) {
      return model.activityLog.search('', '', startIndex, rowCount);
    };



  // returns a promise (not an Item - see demandXyz() for the delayed load Item variant)
    self.ensureActivityLog = function (itemKey) {
      var modelItems = model.activityLog;
      var item = modelItems.hashMap[itemKey];
      if (!item) {
        return modelItems.search("%" + itemKey, "", 0, 1)
        .then(function (ignoredNotificationData) {
          return modelItems.hashMap[itemKey];
        });
      }

      return $q.when(item);
    };





    this.updateAuthUserAway = function (away) {
        return utilityHub.server.updateAuthUserAway(away);
    }

    utilityHub.on('setAuthenticatedTenantGroup', function (tenantGroupsData) {
      $rootScope.$apply(onSetAuthenticatedTenantGroup(tenantGroupsData));
    }).on('setAuthenticatedUser', function (usersData) {
      $rootScope.$apply(onSetAuthenticatedUser(usersData));
    }).on('setSystemAuthenticated', function (tenantID) {
      $rootScope.$apply(onSetSystemAuthenticated(tenantID));
    }).on('updateJobsData', function (jobsData) {
      $rootScope.$apply(onJobsDataUpdated(jobsData));
    }).on('updateTenantGroups', function (itemsData) {
      $rootScope.$apply($rootScope.$broadcast('updateTenantGroups', onTenantGroupsUpdated(model.tenantGroups, itemsData)));
    }).on('updateUsers', function (itemsData) {
      $rootScope.$apply($rootScope.$broadcast('updateUsers', onUsersUpdated(model.users, itemsData)));
    }).on('updateMyTasks', function (itemsData) {
      $rootScope.$apply($rootScope.$broadcast('updateMyTasks', onMyTasksUpdated(itemsData)));
    }).on('updateConversations', function (conversationsData) {
      $rootScope.$apply(onConversationsUpdated(conversationsData, true));
    }).on('updateAuthTokens', function (authTokensData) {
      // rebroadcast out to any controllers that are listening
      $rootScope.$apply($rootScope.$broadcast('updateAuthTokens:', authTokensData));
    }).on('updateEventLog', function (logData) {
      $rootScope.$apply($rootScope.$broadcast('updateEventLog', onEventLogUpdated(model.eventLog, logData)));
    }).on('updateDatabaseLog', function (logData) {
      $rootScope.$apply(onDatabaseLogUpdated(logData, true));
    }).on('resetConnection', function () {
      $rootScope.$apply(resetConnection());
    }).on('serverRestart', function () {
      $rootScope.$apply(self.onServerRestart());
    });

    function onSetAuthenticatedTenantGroup(tenantGroupsData) {
      onTenantGroupsUpdated(model.tenantGroups, tenantGroupsData);
      if (tenantGroupsData && tenantGroupsData.items) {
        model.authenticatedGroup = tenantGroupsData.items[0];

        msAuthenticated.setAuthenticatedGroup(model.authenticatedGroup);


      }
    };

    function onSetAuthenticatedUser(usersData) {
      // usersData is expected to contain just one user - the authenticated user
      onUsersUpdated(model.users, usersData);
      if (usersData && usersData.items) {

        //!! TODO - this users' data might change - need to track that in onUsersUpdated() - but do that after we change the server notification
        var authenticatedUser = usersData.items[0];
        model.authenticatedIdentity = msIdentity.createUser(authenticatedUser);
        //!! retire this guy...
        model.authenticatedUser = authenticatedUser;

        msAuthenticated.setAuthenticatedIdentity(model.authenticatedIdentity);

        $rootScope.$broadcast('authenticated:', model.authenticatedIdentity);
      }
    };

    function onSetSystemAuthenticated(tenantID) {
      var appRoles = ['Admin'];
      var systemRoles = ["SystemAdmin", "TenantAdmin"];
      model.authenticatedIdentity = msIdentity.create('tenant', tenantID, "System Admin", appRoles, systemRoles, null);
      model.authenticatedUser = null;

        msAuthenticated.setAuthenticatedIdentity(authenticatedUser);


      $rootScope.$broadcast('authenticated:', model.authenticatedIdentity);
    };



























  function onDatabaseLogUpdated(logData, isExternalEvent) {
    purgeItems(logData.deletedIDs, model.databaseLog, model.databaseLogIndex);
    var newItemIDs = cacheItems(logData.items, model.databaseLog, model.databaseLogIndex);

    var notification = {
      hashMap: model.databaseLog,
      ids: $.map(logData.items, function (item) {
        return item.id;
      }),
      newIDs: newItemIDs,
      deletedIDs: logData.deletedIDs,
      resolvedIDs: []
    };

    if (!isExternalEvent) {
      notification.totalCount = logData.totalCount;
    }

    if (isExternalEvent) {
      $rootScope.$broadcast('updateDatabaseLog', notification);
    }

    return notification;
  };


  this.doCommand = function (command, commandData) {
    return callHub(function () {
      return siteHub.server.doCommand(command, commandData);
    });
  };

  $.connection.hub.error(function (error) {
    console.log('SignalR error: ' + error)
  });

  $.connection.hub.connectionSlow(function () {
    $rootScope.$apply(function () {
      model.serverConnectionSlow = true;
    });
    clearTimeout(slowTimer);
    slowTimer = setTimeout(function () {
      if ($.connection.hub && $.connection.hub.state === $.signalR.connectionState.connected ||
          $.connection.hub && $.connection.hub.state === $.signalR.connectionState.reconnected) {
        model.serverConnectionSlow = false;
      }
    }, 30000);
  });

    $.connection.hub.reconnecting(function () {

      $rootScope.$applyAsync(function () {
        // Remember our current state

        connectionLostState = {
          stateName: $state.current.name,
          params: angular.extend({}, $state.params)
        };

        model.serverConnectionStatus = CONNECTION_STATUS.reconnecting;

        $state.go('public.reconnecting', {}, { location: false });
      });
    });

    $.connection.hub.disconnected(function () {
      if ($.connection.hub.lastError) {
        $log.log("Disconnected. Reason: " + $.connection.hub.lastError.message);
      }
      if (hubReady) {
        // This is an unplanned disconnection - steer the application through this rough patch
        $rootScope.$applyAsync(function () {
          var test = connectionLostState;
          model.serverConnectionStatus = CONNECTION_STATUS.disconnected;
          $state.go('public.disconnected', {}, { location: false });
        });
      }
    });


    self.restartConnection = function () {


      // Instantiating the UtilityService creates the initial SignalR hub connection. If that connection is lost, we end
      // up in state "public.disconnected". This method provides a way out of that disconnected state.

      var currentState = $state.current;

      if ($.connection.hub.state !== $.signalR.connectionState.disconnected ||
        !$state.current ||
        $state.current.name !== "public.disconnected") {
        // disallow if we're not in the correct starting state
        return;
      }

      //!! hide the button or change states!
      // $state.go('public.restartConnection');

      startConnection()
      .done(function () {
        if (connectionLostState) {
          $state.go(connectionLostState.stateName, connectionLostState.params);
        }
        else {
          // shouldn't happen
          $state.go('public.landing');
        }
      }).fail(function (reason) {
        //!! show the button or change states
        $state.go('public.disconnected');
      });
    };

    $.connection.hub.reconnected(function () {
      $rootScope.$applyAsync(function () {
        model.serverConnectionStatus = CONNECTION_STATUS.online;
        model.serverConnectionSlow = false;

        if (connectionLostState) {
          $state.go(connectionLostState.stateName, connectionLostState.params);
        }
        else {
          // shouldn't happen
          $state.go('public.landing');
        }
      });
    });

    function startConnection() {

      //!! ideally we should be creating a new model object following each successful connection to the server - so we don't leak any private/stale data from the previous connection
      $rootScope.$broadcast(CONNECTION_EVENT.connectionStarting);

      model.authenticatedIdentity = null;
      model.authenticatedUser = null;

      // Starting a new connection - ensure we don't leak any data fetched by the last user/connection
      model.users.hashMap = {};
      model.users.index = [];
      model.users.activeUsersIndexer.index = [];
      model.users.disabledUsersIndexer.index = [];






      // helpful so the server can decide if it can accept this client. The server might send a resetConnection() or onServerRestart() notification if the client code is stale
      $.connection.hub.qs = { version: "1.0", serverStartTimeSeconds: SYSTEM_INFO.serverStartTimeSeconds };


      //!! SignalR logging
      $.connection.hub.logging = true;
      hubReady = $.connection.hub.start()
          .done(function () {
            // (happens after SetAuthenticatedIdentity() is called)
            model.serverConnectionStatus = CONNECTION_STATUS.online;
            model.serverConnectionSlow = false;

            $rootScope.$broadcast(CONNECTION_EVENT.connectionStarted);
          }).fail(function (reason) {
            console.log("SignalR connection failed: " + reason);
          });
      return hubReady;
    };


    function stopConnection() {
      model.authenticatedIdentity = null;
      model.authenticatedUser = null;

      // track that our hub is no longer accessible (because of this planned stop)
      hubReady = null
      $.connection.hub.stop();
      $rootScope.$broadcast(CONNECTION_EVENT.connectionStopped);
    };

    function resetConnection() {
        // this helps when a client has remained active (and hopes to reconnect) but the server has been restarted
        stopConnection();
        startConnection();
    };
    this.resetConnection = resetConnection;





    self.onServerRestart = function () {
      // If the server has restarted, we need to reload too - to freshen our SPA
      stopConnection();
      $window.location.reload();
    };


    this.whenConnected = function () {
      return hubReady;
    }

    // ** Utility functions


    function arrayAdd(id, indexArray) {
      // Ensure id IS in array index
      if ($.inArray(id, indexArray) < 0) {
        indexArray.push(id);
      }
    };
    this.arrayAdd = arrayAdd;

    // Ensure 'value' doesn't occur in array. If value is an array, ensure none of those values occur in array.
    function arrayRemove(array, value) {

      if (value.constructor === Array) {
        $.each(value, function (arrayValueIndex, arrayValue) {
          arrayRemove(array, arrayValue);
        });
        return;
      }

      // Ensure id is NOT in array index
      var index = $.inArray(value, array);
      if (index > -1) {
        array.splice(index, 1);
      }
    };
    this.arrayRemove = arrayRemove;

    function arrayAddRemove(id, addIndexArray, removeIndexArray1, removeIndexArray2) {
      arrayAdd(id, addIndexArray);
      var removeIndexArrays = Array.prototype.slice.call(arguments, 2);
      $.each(removeIndexArrays, function (removeIndexArrayIndex, removeIndexArray) {
        arrayRemove(removeIndexArray, id);
      });
    };
    this.arrayAddRemove = arrayAddRemove;



    self.createModelItems = function (searchHandler) {
      var modelItems = {
        hashMap: {},
        index: [],
      };

      // Add a 'search' handler, which fetches items server-side, and caches the results
      modelItems.search = function (searchExpression, sortExpression, startIndex, rowCount) {
        return self.callHub(function () {
          return searchHandler(searchExpression, sortExpression, startIndex, rowCount);
        }).then(function (itemsData) {
          return self.updateItemsModel(modelItems, itemsData);
        });
      };

      // returns an object, whereas ensure() returns a promise.
      modelItems.demand = function (itemKey) {
        if (!itemKey) {
          return null;
        }
        return modelItems.hashMap[itemKey] ||
          (
            modelItems.hashMap[itemKey] = { id: itemKey, code: itemKey, displayTitle: 'loading...' },
            // fetches requested objects that aren't already cached asynchronously
            self.delayLoad2(modelItems, itemKey),
            modelItems.hashMap[itemKey]
          );
      };

      // returns a promise, whereas demand() returns a object.
      modelItems.ensure = function (itemKey) {
        if (!itemKey) {
          return $q.when();
        }
        var item = modelItems.hashMap[itemKey];
        if (!item) {
          return modelItems.search("%" + itemKey, "", 0, 1)
          .then(function (notificationData) {
            // (search should have already cached any results)
            return modelItems.hashMap[itemKey];
          });
        }
        return $q.when(item);
      };

      return modelItems;
    };


    self.delayLoad2 = function (modelItems, itemKey) {

      if (!modelItems.delayLoad) {
        modelItems.delayLoad = {
          busy: false,
          deferred: $q.defer(),
          itemKeys: [],
          search: function (searchTerm) {
            return modelItems.search(searchTerm, "", 0, 999999);
          }
        };
      }

      var delayLoadData = modelItems.delayLoad;

      if (itemKey) {

        //!! could add a check here to see if we're already featching this itemID - and if so, return that promise.

        if (delayLoadData.itemKeys.indexOf(itemKey) < 0) {
          // this is a new itemID - add it to our work Q ...
          delayLoadData.itemKeys.push(itemKey);
          // ... and mark the target with an object - so Angular can reference the final object that will eventually be hydrated.
          if (!modelItems.hashMap[itemKey]) {
            modelItems.hashMap[itemKey] = {};
          }
        }
      }

      if (delayLoadData.busy) {
        //!! huh?
        return delayLoadData.deferred.promise;
      }

      if (delayLoadData.itemKeys.length > 0) {
        // we're going to issue a new request!

        delayLoadData.busy = true;

        var activeDeferred = delayLoadData.deferred;
        var searchTerm = '%' + delayLoadData.itemKeys.join(" %");

        // create a new promise and array for our next batch of work
        delayLoadData.deferred = $q.defer();
        delayLoadData.itemKeys = [];

        delayLoadData.search(searchTerm)
        .then(function () {
          activeDeferred.resolve();

          delayLoadData.busy = false;

          if (delayLoadData.itemKeys.length > 0) {
            self.delayLoad2(modelItems);
          }
        });

        return activeDeferred.promise;
      }
    };











    function cacheSortedItems(items, hashMap, hashMapIndex /*, indexer1, indexer2, ..., indexerN*/) {
      var indexers = Array.prototype.slice.call(arguments, 3);

      // track any new items we're adding to the hashMap
      var newItemsIndex = [];
      if (!items) {
        // nothing to do
        return newItemsIndex;
      }

      // create a customized compareBy that knows how to compare an Item with a Key in our index array
      // Remember, a Key can be an integer (id) or a string (code).
      var rightKeyCompareBy = function (left, rightKey) {
        var keyPropertyName = "id";
        var result = (left[keyPropertyName] < rightKey) ? -1 : (left[keyPropertyName] > rightKey) ? 1 : 0;
        return result;

        //return left.id - rightID;
        //var right = hashMap[rightID];
        //return compareByID(left, right);
      };

      $.each(items, function (index, item) {
        var existingItem = hashMap[item.id];

        // ** Updated our primary hashMapIndex (sorted based on item.ID)
        if (hashMapIndex) {
          // (item IDs are immutable, so we don't have to worry about an item changing location in the hashMapIndex)
          var hashMapIndexOffset = binarySearch(hashMapIndex, item, rightKeyCompareBy);
          if (hashMapIndexOffset < 0) {
            var insertOffset = ~hashMapIndexOffset;
            hashMapIndex.splice(insertOffset, 0, item.id);
          }
        }

        // ** Update any secondary indexers
        $.each(indexers, function (indexerIndex, indexer) {

          var beforeOffset = indexerBinarySearch(indexer, hashMap, existingItem);
          var afterOffset = indexerBinarySearch(indexer, hashMap, item);

          // determine if we need to remove the existing index entry
          if (beforeOffset != null && beforeOffset >= 0) {
            if (beforeOffset === afterOffset || beforeOffset === ~afterOffset || beforeOffset+1 === ~afterOffset) {
              // no sense removing and adding back at the same location
              afterOffset = null;
            } else {
              // remove the existing ID
              indexer.index.splice(beforeOffset, 1);
              if (afterOffset != null) {
                // fixup the afterOffset if it comes later in the list than the item we just removed, to account for the missing beforeID
                if (afterOffset > beforeOffset) {
                  // (should never happen, if all Items are unique, we shouldn't have different before & after Offsets which are both *positive*!)
                  afterOffset--;
                } else if (~afterOffset > beforeOffset) {
                  afterOffset++;
                }
              }
            }
          }
          // determine if we need to add a new index entry
          if (afterOffset != null) {
            var insertOffset = afterOffset >= 0 ? afterOffset : ~afterOffset;
            indexer.index.splice(insertOffset, 0, item.id);
          }
        });

        // ** Update the actual hashMap (so we have accurate records of the data we're storing)
        //    And update newItemsIndex (so we can send out an accurate notification of any new items that have been added)
        if (existingItem) {
          angular.extend(existingItem, item);
        } else {
          hashMap[item.id] = item;
          // (new items assumed to come in default sort order from the server, so append to the end)
          newItemsIndex.push(item.id);
        }
      });
      return newItemsIndex;

    };

    function indexerBinarySearch(indexer, hashMap, item) {
      if (!item) {
        return null;
      }
      if (indexer.filter && !indexer.filter(item)) {
        return null;
      }
      return hashMapBinarySearch(indexer.index, item, indexer.sort, hashMap);
    }

    function indexMerge(destinationIndex, source, hashMap, filterFunction, sortFunction) {
      var indexFilterFunction;
      if (filterFunction) {
        // If we're passed a filterFunction, it knows how to determine if an item is filtered or not. But we've got an itemID.
        // So create a customized filterFunction that knows how to dereference items from the hashMap.
        indexFilterFunction = function (itemID) {
          var item = hashMap[itemID];
          return filterFunction(item);
        }
      }

      var indexSortFunction;
      if (sortFunction) {
        // If we're passed a sortFunction, it knows how to compare two items. But we've got two itemIDs.
        // So create a customized sortFunction that knows how to dereference items from the hashMap.
        indexSortFunction = function (leftID, rightID) {
          var left = hashMap[leftID];
          var right = hashMap[rightID];
          return sortFunction(left, right);
        }
      }

      indexMergeInternal(destinationIndex, source, indexFilterFunction, indexSortFunction);
    }
    this.indexMerge = indexMerge;

    function indexMergeInternal(destinationIndex, source, indexFilterFunction, indexSortFunction) {
      if (source && source.constructor === Array) {
        // If we're passed an array of values, recursively process each one
        $.each(source, function (sourceArrayValueIndex, sourceArrayValue) {
          indexMergeInternal(destinationIndex, sourceArrayValue, indexFilterFunction, indexSortFunction);
        });
        return;
      }
      // perform a sorted insert

      // should this item be included in our Index? (Default to yes if we don't have an indexFilterFunction)
      var includeInIndex = indexFilterFunction ? indexFilterFunction(source) : true;

      // where should this item be inserted? (Default to the end if we don't have a indexSortFunction)
      var destinationIndexOffset = indexSortFunction ? binarySearch(destinationIndex, source, indexSortFunction) : ~destinationIndex.length;
      if (destinationIndexOffset < 0) {
        if (includeInIndex) {
          // not currently in Index but required to be ... so insert it
          var insertOffset = ~destinationIndexOffset;
          destinationIndex.splice(insertOffset, 0, source);
        } else {
          // not currently in Index and not required to be ... so nothing to do
        }
      } else {
        if (includeInIndex) {
          // currently in Index and required to be in the Index ... so think about a resort?
          if (destinationIndexOffset < destinationIndex.length && destinationIndex[destinationIndexOffset] == source) {
            // noting to do - the item is already in the correct spot
          } else {
            // 
            // Something changed about an element in our list that caused it to change position.
            // TODO: Seems like the correct response is to move it to the new correct position.
            // CONSIDER: But what if tons of stuff has changed - when do we panic and just do a complete resort?
            console.log("resort?", source);
          }
        } else {
          // currently in Index but not required to be ... so remove it
          destinationIndex.splice(destinationIndexOffset, 1);
        }
      }
    }


  //!! this is deprecated in favor of cacheSortedItems()
    function cacheItems(items, hashMap, hashMapIndex, compareBy) {
      // track any new items we're adding to the hashMap
      var newItemsIndex = [];
      if (!items) {
        // nothing to do
        return newItemsIndex;
      }

      // create a customized compareBy that knows how to compare an Item with an ID in our index array
      compareBy = compareBy || compareByID;
      var rightIDCompareBy = function (left, rightID) {
        var right = hashMap[rightID];
        return compareBy(left, right);
      }

      $.each(items, function (index, item) {
        var existingItem = hashMap[item.id];
        if (existingItem) {
          angular.extend(existingItem, item);
        } else {
          hashMap[item.id] = item;
          // (new items assumed to come in default sort order from the server, so append to the end)
          newItemsIndex.push(item.id);
        }

        if (hashMapIndex && compareBy) {
          // perform a sorted insert
          var hashMapIndexOffset = binarySearch(hashMapIndex, item, rightIDCompareBy);
          if (hashMapIndexOffset < 0) {
            var insertOffset = ~hashMapIndexOffset;
            hashMapIndex.splice(insertOffset, 0, item.id);
          }
        }
      });
      return newItemsIndex;
    }
    this.cacheItems = cacheItems;

    function hashMapBinarySearch(ar, el, compare_fn, hashMap) {
      return binarySearch(ar, el, function (left, rightID) {
        var right = hashMap[rightID];
        return compare_fn(left, right);
      });
    }

  //!! consider adding an outer optimization which looks to see if the item is added first/last - as is the very common case


  /*
   * Binary search in JavaScript.
   * Returns the index of of the element in a sorted array or (-n-1) where n is the insertion point for the new element.
   * Parameters:
   *     ar - A sorted array
   *     el - An element to search for
   *     compare_fn - A comparator function. The function takes two arguments: (a, b) and returns:
   *        a negative number  if a is less than b;
   *        0 if a is equal to b;
   *        a positive number of a is greater than b.
   * The array may contain duplicate elements. If there are more than one equal elements in the array, 
   * the returned value can be the index of any one of the equal elements.
   */
    function binarySearch(ar, el, compare_fn) {
      var m = 0;
      var n = ar.length - 1;
      while (m <= n) {
        var k = (n + m) >> 1;
        var cmp = compare_fn(el, ar[k]);
        if (cmp > 0) {
          m = k + 1;
        } else if (cmp < 0) {
          n = k - 1;
        } else {
          return k;
        }
      }
      return -m - 1;
    }

    function purgeItems(purgeIDs, hashMap /*, indexArrays ...*/) {
        if (!purgeIDs) return;

        var indexArrays = Array.prototype.slice.call(arguments, 2);

        $.each(purgeIDs, function (index, purgeID) {
            // Remove the id from any indexArrays
            $.each(indexArrays, function (indexArrayIndex, indexArray) {
                var purgeIndexIndex = $.inArray(purgeID, indexArray);
                if (purgeIndexIndex >= 0) {
                    indexArray.splice(purgeIndexIndex, 1);
                }
            });
            // Remove the object from the store
            delete hashMap[purgeID];
        });
    }
    this.purgeItems = purgeItems;


    self.updateItemsModel = function(itemsModel, itemsData) {


      // Purge any Indexers we've got


      // Update our main cache (model.xxx) and cacheIndex (model.xxxIndex)
      var purgeItemsArguments = [itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index];
      if (itemsModel.indexers) {
        purgeItemsArguments.push.apply(purgeItemsArguments, itemsModel.indexers);
      }
      self.purgeItems.apply(self, purgeItemsArguments);
      //!!self.purgeItems(itemsData.deletedIDs, itemsModel.hashMap, itemsModel.index);


      var cacheItemsArguments = [itemsData.items, itemsModel.hashMap, itemsModel.index];
      if (itemsModel.indexers) {
        cacheItemsArguments.push.apply(cacheItemsArguments, itemsModel.indexers);
      }
      var newItemIDs = cacheSortedItems.apply(self, cacheItemsArguments);
      //!!var newItemIDs = self.cacheItems(itemsData.items, itemsModel.hashMap, itemsModel.index);

      var notification = {
        hashMap: itemsModel.hashMap,
        ids: $.map(itemsData.items, function (item) {
          return item.id;
        }),
        newIDs: newItemIDs,
        deletedIDs: itemsData.deletedIDs,
        totalCount: itemsData.totalCount,
        resolvedIDs: []
      };

      return notification;
    };








  //!! INDEXER Helpers - so that controllers can keep things up-to-date based on the standard notifications we send from our services!! 

    self.registerIndexer = function (modelItems, indexer) {

      var modelItemsIndexers = modelItems.indexers;
      if (!modelItemsIndexers) {
        modelItems.indexers = modelItemsIndexers = [];
      }

      //!! should check if we really need to add it!!!
      modelItemsIndexers.push(indexer);
    };

    self.unRegisterIndexer = function (modelItems, indexer) {
      self.arrayRemove(modelItems.indexers, indexer);
    };










    function callHub(hubRequest) {


      if (!hubReady) {
        // Hmm. Our server connection isn't started. So this is never gonna work.
        $log.log("CallHub on a stopped connection...");

        //!! should we $state.go somewhere?

      }

      var deferred = $q.defer();


        hubReady.done(function () {
          hubRequest()
              .progress(function (progress) {
                //console.log(progress);
                deferred.notify(progress);
                // note since this data comes from outside Angular scope, we have to use $apply()
                //$rootScope.$apply();
                //!! not sure if this is
                return progress;
              })
              .done(function (hubResult) {
                  if (hubResult.StatusCode == 200) {
                    // pass on just the response data.
                    deferred.resolve(hubResult.ResponseData);
                    // note since this data comes from outside Angular scope, we have to use $apply()
                    //$rootScope.$apply();
                    return hubResult.ResponseData;
                  }

                  // pass on the entire response
                  deferred.reject(hubResult);

                  if (hubResult.StatusCode == 401) {
                    //!! hmm - should have have an application defined "safe state" or "unauthorized access" state? Should we be going to a URL "/"?
                    $state.go('master.home', {}, { reload: true });
                  }

                  // note since this data comes from outside Angular scope, we have to use $apply()
                  //$rootScope.$apply();
                  return hubResult;
              });
        });

        return deferred.promise;
    };
    this.callHub = callHub;


    //Init

  //!! todo temporary
    self.createMyTask('Respond to 3 clients');

  //!! I suck. I'm using the time as our ID - and don't want two with the same ID
    var tick = new Date().getTime();
    while (new Date().getTime() == tick)
    {
      var i;
      i = i + 2;
      // do nothing
    }
    self.createMyTask('Complete your TORQ profile');
    tick = new Date().getTime();
    while (new Date().getTime() == tick) {
      var i;
      i = i + 2;
      // do nothing
    }

    self.createMyTask('Create a list');



    // And then we start up the connection
    startConnection();
}]);

app.service('webUtilityService', ['$rootScope', 'utilityService', function ($rootScope, utilityService) {

    var webUtilityHub = $.connection.webUtilityHub; // the generated client-side hub proxy

    this.model = utilityService.getModel();

    // WebUtilityHub methods

    this.sendSetPasswordEmail = function (email) {
      return utilityService.callHub(function () {
        return webUtilityHub.server.sendSetPasswordEmail(email);
      });
    };

    this.sendResetPasswordEmail = function (email) {
        return utilityService.callHub(function () {
            return webUtilityHub.server.sendResetPasswordEmail(email);
        });
    };
}]);

app.directive('fsmStickyHeader', [function () {
  return {
    restrict: 'EA',
    replace: false,
    scope: {
      scrollBody: '=',
      scrollStop: '=',
      scrollableContainer: '=',
      contentOffset: '='
    },
    link: function (scope, element, attributes, control) {
      var header = $(element, this);
      var clonedHeader = null;
      var content = $(scope.scrollBody);
      var scrollableContainer = $(scope.scrollableContainer);
      var contentOffset = scope.contentOffset || 0;

      if (scrollableContainer.length === 0) {
        scrollableContainer = $(window);
      }

      function setColumnHeaderSizes() {
        if (clonedHeader.is('tr') || clonedHeader.is('thead')) {
          var clonedColumns = clonedHeader.find('th');
          header.find('th').each(function (index, column) {
            var clonedColumn = $(clonedColumns[index]);
            clonedColumn.css('width', column.offsetWidth + 'px');
          });
        }
      };

      function determineVisibility() {
        var scrollTop = scrollableContainer.scrollTop() + scope.scrollStop;
        var contentTop = content.offset().top + contentOffset;
        var contentBottom = contentTop + content.outerHeight(false);

        if ((scrollTop > contentTop) && (scrollTop < contentBottom)) {
          if (!clonedHeader) {
            createClone();
            clonedHeader.css({ "visibility": "visible" });
          }

          if (scrollTop < contentBottom && scrollTop > contentBottom - clonedHeader.outerHeight(false)) {
            var top = contentBottom - scrollTop + scope.scrollStop - clonedHeader.outerHeight(false);
            clonedHeader.css('top', top + 'px');
          } else {
            calculateSize();
          }
        } else {
          if (clonedHeader) {
            /*
             * remove cloned element (switched places with original on creation)
             */
            header.remove();
            header = clonedHeader;
            clonedHeader = null;

            header.removeClass('fsm-sticky-header');
            header.css({
              position: 'relative',
              left: 0,
              top: 0,
              width: 'auto',
              'z-index': 0,
              visibility: 'visible'
            });
          }
        }
      };

      function calculateSize() {
        clonedHeader.css({
          top: scope.scrollStop,
          width: header.outerWidth(),
          left: header.offset().left
        });

        setColumnHeaderSizes();
      };

      function createClone() {
        /*
         * switch place with cloned element, to keep binding intact
         */
        clonedHeader = header;
        header = clonedHeader.clone();
        clonedHeader.after(header);
        clonedHeader.addClass('fsm-sticky-header');
        clonedHeader.css({
          position: 'fixed',
          'z-index': 10000,
          visibility: 'hidden'
        });
        calculateSize();
      };

      scrollableContainer.on('scroll.fsmStickyHeader', determineVisibility).trigger("scroll");
      scrollableContainer.on('resize.fsmStickyHeader', determineVisibility);

      scope.$on('$destroy', function () {
        scrollableContainer.off('.fsmStickyHeader');
      });
    }
  };
}]);

app.directive('msSearchView', function ($parse, utilityService) {
  return {
    restrict: 'EA',
    // Define our own isolated scope. We only share the specified variables with our host.
    scope: {
      options: '='
    },
    transclude: true,
    replace: true,
    controller: function ($scope, utilityService) {

      $scope.loadPageBusy = false;
      $scope.loadPageDone = false;
      $scope.loadPageSearchExpression = null;
      $scope.loadPageSortExpression = null;

      $scope.displayedIndex = [];
      $scope.totalCount = null;

      $scope.updateClientFilter = function () {

        var clientFilters = utilityService.buildClientFilter(
          $scope.options.baseFilter,
          $scope.options.stackFilters,
          $scope.options.selectFilter,
          $scope.options.objectFilter,
          $scope.options.userSearch);


        $scope.clientFilterFunction = function (item) {

          // If we fail any filter, return false
          for (var i = 0; i < clientFilters.length; i++) {
            if (!clientFilters[i](item)) {
              return false;
            }
          }

          // Otherwise return true
          return true;
        }
      }


      $scope.onChangeEvent = function (eventData) {

        // Handle deleted items - no point showing the user things that don't exist any more
        utilityService.arrayRemove($scope.displayedIndex, eventData.deletedIDs);

        // account for totalCount! did we count these before or not?
        if ($scope.totalCount && eventData.deletedIDs) {
          $scope.totalCount -= eventData.deletedIDs.length;
        }

        // Now we have a choice - what to do about objects that are changing. At a minimum their UI should update to reflect reality. But what else?
        // Should they disappear if they no longer qualify to be included in the solution set?


        if ($scope.options.userSearch) {
          // When we have a userSearch term our list is FROZEN for inserts - as we don't know how to filter on that search term locally
          return;
        }

        // LIVE view - we're interested in seeing new items show up in our list

        // Establish our hashMap - which contains our item objects.
        // Our preference is to use the hashMap specified in the event (if any). Then it doesn't need to be specified as a directive attribute
        // Otherwise fall back on the hashMap specified as a directive attribute
        var hashMap = eventData.hashMap || $scope.hashMap;

        //!! utilityService.indexMerge($scope.displayedIndex, eventData.ids, hashMap, $scope.options.filter.clientFunction, $scope.options.sort.clientFunction);
        utilityService.indexMerge($scope.displayedIndex, eventData.ids, hashMap, $scope.clientFilterFunction, $scope.options.sort.clientFunction);

        if ($scope.totalCount) {
          $scope.totalCount += eventData.ids.length;
        }
      };


      $scope.loadPage = function (searchExpression) {
        if ($scope.loadPageBusy) return;
        $scope.loadPageBusy = true;

        var searchStartIndex = $scope.displayedIndex.length;


        var requestedSearchExpression = utilityService.buildSearchExpression(
          $scope.options.baseFilter,
          $scope.options.stackFilters,
          $scope.options.selectFilter,
          $scope.options.objectFilter,
          $scope.options.userSearch);

        var requestedSortExpression = $scope.options.sort.serverTerm;

        if ($scope.loadPageSortExpression !== requestedSortExpression || $scope.loadPageSearchExpression !== requestedSearchExpression) {
          // our filter expression has changed - remember the new one
          $scope.loadPageSortExpression = requestedSortExpression;
          $scope.loadPageSearchExpression = requestedSearchExpression;

          // and search again from the top, establishing a new resultSet.
          searchStartIndex = 0;

          if ($scope.loadPageSearchExpression) {
            // FILTERED - new search expression, reset our current results
            searchStartIndex = 0;
          } else {
            // UNFILTERED - first time through, or we're transitioning from filtered to unfiltered
            //!! only grab a screen full of data
            //!!! $scope.displayedIndex = $scope.model.dbRequestLogIndex;
            //!!!  $scope.totalCount = $scope.model.dbRequestLogTotalCount;

            if ($scope.displayedIndex.length > 0) {
              // transitioning from filtered to unfiltered
              //!!   return;
            }
          }
        };



        //!! Alternative way to call and pass parameters to a directive provided callback function
        //var params = { searchExpression: $scope.loadPageSearchExpression, startIndex: searchStartIndex, rowCount: 32 };
        //$scope.searchPage($scope, params)

        $scope.searchHandler($scope.loadPageSearchExpression, $scope.loadPageSortExpression, searchStartIndex, 32)
            .then(function (searchData) {

              if (searchStartIndex === 0) {
                // We're starting the first page of a filtered set - reset results back to empty
                $scope.displayedIndex = [];
              }

              if ($scope.loadPageSearchExpression) {
                // FILTERED
                if (searchStartIndex == 0) {
                  // We're starting the first page of a filtered set - reset results back to empty
                  $scope.displayedIndex = [];
                }
              }

              // establish our hashMap - which contains our item objects
              var hashMap = searchData.hashMap || $scope.hashMap;




              if ($scope.displayedIndex && $scope.displayedIndex.length) {
                //!! utilityService.indexMerge($scope.displayedIndex, searchData.ids, hashMap, $scope.options.filter.clientFunction, $scope.options.sort.clientFunction);
                utilityService.indexMerge($scope.displayedIndex, searchData.ids, hashMap, $scope.clientFilterFunction, $scope.options.sort.clientFunction);
              } else {
                // Special case loading the first page - no need to sort and/or filter as the server just did that for us
                utilityService.indexMerge($scope.displayedIndex, searchData.ids);
              }

              // freshen our totalCount stat - on the assumption that server knows best
              $scope.totalCount = searchData.totalCount;


              //$scope.$hashMap = hashMap;
              //$scope.$parent.$hashMap = hashMap;
              //$scope.$parent.mssv = "234";
              $scope.transcludedScope.$hashMap = hashMap;
              $scope.transcludedScope.$displayedIndex = $scope.displayedIndex;
              //$scope.transcludedScope.displayedIndex = $scope.displayedIndex;
              $scope.transcludedScope.$totalCount = $scope.totalCount;




              $scope.loadPageBusy = false;
              $scope.loadPageDone = $scope.totalCount <= $scope.displayedIndex.length;

              if ($scope.loadPageAgain) {
                $scope.loadPageAgain = false;
                $scope.loadPage();
              }
            });
      };

      $scope.reload = function () {

        $scope.updateClientFilter();

        if ($scope.loadPageBusy) {
          $scope.loadPageAgain = true;
        } else {
          $scope.loadPage();
        }
      };

    },
    link: function (scope, element, attrs, ctrl, transclude) {
      // Required: Parameter that provides our Search() function
      var searchParse = $parse(attrs.search);
      scope.searchHandler = searchParse(scope.$parent);

      if (attrs.hashMap) {
        // Optional: Parameter for looking up items via the IDs returned from the Search() function
        var hashMapParse = $parse(attrs.hashMap);
        scope.hashMap = hashMapParse(scope);
      }
      // Optional: Event name to listen for to receive update notifications
      scope.changeEvent = attrs.changeEvent;

      if (!attrs.options) {
        // if we weren't provided options, default to an empty object instead of undefined
        scope.options = { sort: {}, filter: {} };
      } else {
        scope.$watch('options.sort', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        });

        // note 'deep' watch used on some of these ...

        scope.$watch('options.baseFilter', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        }, true);

        scope.$watch('options.stackFilters', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        }, true);

        scope.$watch('options.selectFilter', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        });

        scope.$watch('options.objectFilter', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        }, true);

        scope.$watch('options.userSearch', function (newValue, oldValue, scope) {
          if (newValue !== oldValue) {
            scope.reload();
          }
        });

        scope.updateClientFilter();

      }

      scope.$on(scope.changeEvent, function (event, eventData) {
        scope.onChangeEvent(eventData);
      });

      //transclude(scope, function (clone, scope) {
//        element.find('.ms-search-view').append(clone);
      //});
      transclude(function (clone, transcludedScope) {
        scope.transcludedScope = transcludedScope;
        element.find('.ms-search-view').append(clone);
      });
    },

    template: function(element, attrs) {
      var container = attrs.hasOwnProperty('container') ? ' infinite-scroll-container="' + attrs.container + '"' : '';

      var templateHtml =
        '<div>' +
          // 'Search: <input type="text" class="form-control" placeholder="Search for any text, &commat;name, or even &num;tag" ng-model="userSearch">' +
          '<div class="ms-search-view"' + container + ' data-infinite-scroll="loadPage()" data-infinite-scroll-disabled="loadPageDone" data-infinite-scroll-distance="2" data-infinite-scroll-immediate-check="true">' +
          '</div>' +
        '</div>';

      return templateHtml;
    }
  };
});

app.directive('passwordMatch', [function () {
  return {
    restrict: 'A',
    scope: true,
    require: 'ngModel',
    link: function (scope, elem, attrs, control) {
      var checker = function () {

        //get the value of the first password
        var e1 = scope.$eval(attrs.ngModel);

        //get the value of the other password
        var e2 = scope.$eval(attrs.passwordMatch);
        return e1 == e2;
      };
      scope.$watch(checker, function (n) {

        //set the form control to valid if both
        //passwords are the same, else invalid
        control.$setValidity("unique", n);
      });
    }
  };
}]);

// http://stackoverflow.com/questions/20325480/angularjs-whats-the-best-practice-to-add-ngif-to-a-directive-programmatically
function ngIfVariation(ngIfDirective, customLink) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority - 1,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function (scope, element, attributes) {
      // grab our custom ng-if function
      var customNgIf = customLink(scope, element, attributes);
      // grab the initial ng-if attribute (if any)
      var initialNgIf = attributes.ngIf, ifEvaluator;
      if (initialNgIf) {
        ifEvaluator = function () {
          // ng-if exists! evaluate both
          return scope.$eval(initialNgIf) && customNgIf();
        };
      } else {
        ifEvaluator = function () {
          // ng-if doesn't exist, just use our custom one
          return customNgIf();
        };
      }
      attributes.ngIf = ifEvaluator;
      ngIf.link.apply(ngIf, arguments);
    }
  };
}

app.directive('msDebugOnly', function (ngIfDirective, $stateParams) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    return function () {
      return $stateParams.debug;
    };
  });
});

app.directive('msFacadeOnly', function (ngIfDirective, $stateParams) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    return function () {
      return $stateParams.facade;
    };
  });
});

app.directive('msShow', function (ngIfDirective, msAuthenticated) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    var myAttribute = scope.$eval(attributes.msShow);
    return function () {
      return msAuthenticated.authorizeRole(myAttribute);
    };
  });
});

app.directive('msHide', function (ngIfDirective, msAuthenticated) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    var myAttribute = scope.$eval(attributes.msHide);
    return function () {
      return !msAuthenticated.authorizeRole(myAttribute);
    };
  });
});



// ms-show ms-not-got-it ms-spa

// ms-production
// ms-development
// ms-spa="user"
// ms-spa="volunteer"
// ms-spa="!volunteer"

app.directive('msDevelopment', function (ngIfDirective, SYSTEM_INFO) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    return function () {
      return SYSTEM_INFO.isDevelopmentSite;
    };
  });
});

app.directive('msProduction', function (ngIfDirective, SYSTEM_INFO) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    return function () {
      return SYSTEM_INFO.isProductionSite;
    };
  });
});


app.directive('msSpa', function (ngIfDirective, SYSTEM_INFO) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {
    var myAttribute = scope.$eval(attributes.msSpa);
    if (myAttribute.charAt(0) === '!') {
      myAttribute = myAttribute.substr(1);
      return function () {
        return SYSTEM_INFO.spaName !== myAttribute;
      };
    }
    return function () {
      return SYSTEM_INFO.spaName === myAttribute;
    };
  });
});










app.directive('msNotGotIt', function (ngIfDirective, utilityService, msAuthenticated) {
  return ngIfVariation(ngIfDirective, function (scope, element, attributes) {

    scope.setGotIt = function (gotItLabel) {
      // optimistic clear it first
      msAuthenticated.setGotIt(gotItLabel);

      if (msAuthenticated.identity && msAuthenticated.identity.userID) {
        utilityService.setUserGotIt(msAuthenticated.identity, gotItLabel);
      }
    };

    var myAttribute = scope.$eval(attributes.msNotGotIt);
    return function () {
      return msAuthenticated.isNotGotIt(myAttribute);
    };
  });
});






app.directive('autoSaveForm', function ($timeout) {

  return {
    require: ['^form'],
    link: function ($scope, $element, $attrs, $ctrls) {

      var $formCtrl = $ctrls[0];
      var savePromise = null;
      var expression = $attrs.autoSaveForm || 'true';

      $scope.$watch(function () {

        if ($formCtrl.$valid && $formCtrl.$dirty) {

          if (savePromise) {
            $timeout.cancel(savePromise);
          }

          savePromise = $timeout(function () {

            savePromise = null;

            // Still valid?

            if ($formCtrl.$valid) {

              $formCtrl.$saving = true;
              $formCtrl.$setPristine();

              console.log('Form data persisting');

              $scope.$eval(expression)
              .finally(function () {
                console.log('Form data persisted');
                $formCtrl.$saving = undefined;
              });
            }

          }, 500);
        }

      });
    }
  };

});



app.filter('demandUser', ['utilityService', function (utilityService) {
  return function (objectID) {
    return utilityService.demandUser(objectID);
  };
}]);

app.filter('objectLookup', function () {
    return function (objectIDs, objectStore) {
        var objects = [];
        angular.forEach(objectIDs, function (objectID) {
            objects.push(objectStore[objectID]);
        });
        return objects;
    }
});

app.filter('bytes', function () {
    return function (bytes, precision) {
        if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
        if (typeof precision === 'undefined') precision = 1;
        var units = ['bytes', 'kB', 'MB', 'GB', 'TB', 'PB'],
        number = Math.floor(Math.log(bytes) / Math.log(1024));
        return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
    }
});


app.filter('durationSeconds', ['moment', function (moment) {
    return function (value, format, suffix) {
        if (typeof value === 'undefined' || value === null) {
            return '';
        }

        return moment.duration(value, format).asSeconds();
    };
}]);


app.filter('capitalize', function () {
    return function (input, format) {
        if (!input) {
            return input;
        }
        format = format || 'all';
        if (format === 'first') {
            // Capitalize the first letter of a sentence
            return input.charAt(0).toUpperCase() + input.slice(1).toLowerCase();
        } else {
            var words = input.split(' ');
            var result = [];
            words.forEach(function (word) {
                if (word.length === 2 && format === 'team') {
                    // Uppercase team abbreviations like FC, CD, SD
                    result.push(word.toUpperCase());
                } else {
                    result.push(word.charAt(0).toUpperCase() + word.slice(1).toLowerCase());
                }
            });
            return result.join(' ');
        }
    };
});


// 1e32 is enogh for working with 32-bit
// 1e8 for 8-bit (100000000)
// in your case 1e4 (aka 10000) should do it
app.filter('numberFixedLen', function () {
    return function(a,b){
        return(1e4+a+"").slice(-b)
    }
});

app.filter('array', function () {
  return function (items) {
    var filtered = [];
    angular.forEach(items, function (item) {
      filtered.push(item);
    });
    return filtered;
  };
});


//| objectLookup:$hashMap
// sumArray:[1,2,3,4]
app.filter('sumArray', function () {
  return function (inputArray, indexesArray) {
    var sum = 0;
    angular.forEach(indexesArray, function (index) {
      sum += inputArray[index];
    });
    return sum;
  };
});



app.filter('inArray', function () {
  return function (array, value) {
    return array.indexOf(value) !== -1;
  };
});


app.filter('intersect', function () {
  return function (array1, array2) {
    return array1.filter(function (n) {
      return array2.indexOf(n) != -1
    });
  };
});

app.filter('except', function () {
  return function (array1, array2) {
    return array1.filter(function (n) {
      return array2.indexOf(n) == -1
    });
  };
});

function range(start, count) {
  if (arguments.length == 1) {
    count = start;
    start = 0;
  }

  var foo = [];
  for (var i = 0; i < count; i++) {
    foo.push(start + i);
  }
  return foo;
};
