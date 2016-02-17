app.directive('osbCanCheckIn', function ($parse, msAuthenticated, ngIfDirective) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function (scope, element, attrs, ngModel) {

      // If we're passed an expression, save back the ZipCode name
      if (attrs.osbCanCheckIn) {
        var osbCanCheckInParse = $parse(attrs.osbCanCheckIn + '.checkInOpen');
      }

      attrs.ngIf = function () {
        var authenticatedIdentity = msAuthenticated.identity;
        if (!authenticatedIdentity || !authenticatedIdentity.appRoles) {
          // don't all unathenticated to check-in
          return false;
        }
        if (authenticatedIdentity.appRoles.indexOf("Admin") > -1) {
          // Admins can always check-in
          return true;
        }
        if (authenticatedIdentity.appRoles.indexOf("EventPlanner") > -1) {
          // EventPlanner can always check-in
          return true;
        }

        var eventSessionCheckInOpen = osbCanCheckInParse(scope);

        if (authenticatedIdentity.appRoles.indexOf("EventSessionVolunteer") > -1 &&
          eventSessionCheckInOpen) {
          // Volunteers can only check-in when eventSession.checkInOpen
          return true;
        }
        return false;
      };
      ngIf.link.apply(ngIf, arguments);
    }
  };
});

app.directive('osbVolunteerCheckInClosed', function (msAuthenticated, ngIfDirective) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function ($scope, $element, $attr) {
      $attr.ngIf = function () {
        var authenticatedIdentity = msAuthenticated.identity;
        if (authenticatedIdentity && authenticatedIdentity.appRoles && authenticatedIdentity.appRoles.indexOf("Admin") > -1) {
          return true;
        }
        return false;
      };
      ngIf.link.apply(ngIf, arguments);
    }
  };
});

app.directive('osbEventSessionUpcoming', function (msAuthenticated, ngIfDirective) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function ($scope, $element, $attr) {
      $attr.ngIf = function () {
        var authenticatedIdentity = msAuthenticated.identity;
        if (authenticatedIdentity && authenticatedIdentity.appRoles && authenticatedIdentity.appRoles.indexOf("Admin") > -1) {
          return true;
        }
        return false;
      };
      ngIf.link.apply(ngIf, arguments);
    }
  };
});

app.directive('osbEventSessionLive', function (msAuthenticated, ngIfDirective) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function ($scope, $element, $attr) {
      $attr.ngIf = function () {
        var authenticatedIdentity = msAuthenticated.identity;
        if (authenticatedIdentity && authenticatedIdentity.appRoles && authenticatedIdentity.appRoles.indexOf("Admin") > -1) {
          return true;
        }
        //!!
        return true;
      };
      ngIf.link.apply(ngIf, arguments);
    }
  };
});

app.directive('osbEventSessionPast', function (msAuthenticated, ngIfDirective) {
  var ngIf = ngIfDirective[0];
  return {
    transclude: ngIf.transclude,
    priority: ngIf.priority,
    terminal: ngIf.terminal,
    restrict: ngIf.restrict,
    link: function ($scope, $element, $attr) {
      $attr.ngIf = function () {
        var authenticatedIdentity = msAuthenticated.identity;
        if (authenticatedIdentity && authenticatedIdentity.appRoles && authenticatedIdentity.appRoles.indexOf("Admin") > -1) {
          return true;
        }
        return false;
      };
      ngIf.link.apply(ngIf, arguments);
    }
  };
});
