(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('LibraryController', LibraryController);

    LibraryController.$inject = ['$scope', '$log', '$state'];

    /* @ngInject */
    function LibraryController($scope, $log, $state) {
      $log.debug("Loading LibraryController...");

      $scope.currentState = $state.current;

      $scope.imageSources = ['iStockPhoto', 'Flickr', 'Getty Images', 'Shutterstock', 'Adobe', 'Veer', 'Big Stock Photo', 'Corbis', 'Fotolia', 'Little Visuals', 'Unsplash', 'Superfamous', 'Picjumbo', 'Gratisography', 'Public Domain Archive', 'Other'];

      $scope.licenseTypes = ['No license requirements', 'License requirements not fulfilled', 'License requirements fulfilled' ];
    }
})();
