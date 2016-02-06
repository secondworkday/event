app.controller('AppController', function ($scope, $translate, $timeout, $mdSidenav, $mdDialog, $mdUtil, $log, $msUI, $stateParams, $state, utilityService, siteService, AUTHORIZATION_ROLES) {
  $log.debug('Loading AppController...');

  $scope.model = siteService.getModel();

  $scope.months = [
    {name: "January", number: 00},
    {name: "February", number: 01},
    {name: "March", number: 02},
    {name: "April", number: 03},
    {name: "May", number: 04},
    {name: "June", number: 05},
    {name: "July", number: 06},
    {name: "August", number: 07},
    {name: "September", number: 08},
    {name: "October", number: 09},
    {name: "November", number: 10},
    {name: "December", number: 11}
  ];

  $scope.years = [
    2015, 2014, 2013, 2012, 2011, 2010, 2009, 2008, 2007, 2006, 2005, 2004, 2003,
    2002, 2001, 2000, 1999, 1998, 1997, 1996, 1995, 1994, 1993, 1992, 1991, 1990,
    1989, 1988, 1987, 1986, 1985, 1984, 1983, 1982, 1981, 1980, 1979, 1978, 1977,
    1976, 1975, 1974, 1973, 1972, 1971, 1970, 1969, 1968, 1967, 1966, 1965, 1964,
    1963, 1962, 1961, 1960, 1959, 1958, 1957, 1956, 1955, 1954, 1953, 1952, 1951,
    1950
 ];

 $scope.timeZones = [
   "Eastern", "Centeral", "Arizona", "Mountain", "Pacific", "Alaska", "Hawaii"
 ];

 $scope.states = ('AL AK AZ AR CA CO CT DE FL GA HI ID IL IN IA KS KY LA ME MD MA MI MN MS ' +
    'MO MT NE NV NH NJ NM NY NC ND OH OK OR PA RI SC SD TN TX UT VT VA WA WV WI ' +
    'WY').split(' ').map(function(state) {
        return {abbrev: state};
      })

  $scope.debugMode = $stateParams.debug;
  $scope.facadeMode = $stateParams.facade;

  $scope.toggleDebugMode = function () {
    $scope.debugMode = !$scope.debugMode;
    $state.go('.', { debug: $scope.debugMode ? true : undefined });
  };

  $scope.changeLanguage = function (langKey) {
    $translate.use(langKey);
  };


  $scope.goToState = function(state) {
    $state.go(state);
  };

});

app.directive('zipcodeValidator', function ($q, $parse, siteService) {
  return {
    require: 'ngModel',
    link: function (scope, element, attrs, ngModel) {
      // If we're passed an expression, save back the ZipCode name
      if (attrs.zipcodeValidator) {
        var zipCodeNameParse = $parse(attrs.zipcodeValidator);
      }
      ngModel.$asyncValidators.zipcode = function (zipCode) {
        return siteService.getZipCodeInfo(zipCode).then(
          function (zipCodeInfo) {
            if (zipCodeNameParse) {
              zipCodeNameParse.assign(scope, zipCodeInfo.name);
            }
            ngModel.$errorMessage = undefined;
            return zipCodeInfo;
          }, function (errorResponse) {
            if (zipCodeNameParse) {
              zipCodeNameParse.assign(scope, undefined);
            }
            ngModel.$errorMessage = errorResponse.errorMessage;
            return $q.reject(errorResponse);
          });
      };
    }
  };
});
