app.directive('osbVolunteerCheckInOpen', function (msAuthenticated, ngIfDirective) {
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
        return false;
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
