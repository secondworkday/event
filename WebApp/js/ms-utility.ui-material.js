app.service('$msUI', ['$rootScope', '$state', '$mdToast', '$mdDialog', '$timeout', '$log', 'utilityService', function ($rootScope, $state, $mdToast, $mdDialog, $timeout, $log, utilityService) {

  var activeToast, activeToastContent, activeToastContext, activeToastTimeout;
  var self = this;
  var model = utilityService.getModel();


  //** General progress 

  self.showToast = function (content, context) {
    if (activeToastContent === content || (activeToastContext && activeToastContext === context)) {
      // freshen our existing toast - no need to flicker to show a new one
      $mdToast.updateContent(content);
    }
    else {
      activeToast = $mdToast.show(
        $mdToast.simple()
          .content(content)
          .hideDelay(0))
      .finally(function () {
        // this toast dismissed - we can't resuse it anymore
        $timeout.cancel(activeToastTimeout);
        activeToastContent = undefined;
        activeToastContext = undefined;
        activeToastTimeout = undefined;
      });
    }

    activeToastContent = content;
    activeToastContext = context;

    // new or updated content - so reset our timeout
    $timeout.cancel(activeToastTimeout);
    activeToastTimeout = $timeout(function () {
      $mdToast.cancel();
    }, 3000);

    return activeToast;
  };







  //** Conversation related

  this.showComposeMessageDialog = function (ev) {
    $mdDialog.show({
      controller: composeCustomerConversationMessageDialogController,
      templateUrl: '/client/partials/jobseeker/dialog-composeMessage.html',
      targetEvent: ev,
    })
    .then(function () {
      // nothing to do
    }, function () {
      $log.debug('You canceled the message');
    });
  };

  function composeCustomerConversationMessageDialogController($scope, $mdDialog) {

    $scope.send = function () {
      var message = $scope.formData.message;
      utilityService.addCustomerConversationMessage(message)
      .then(function () {
        // Since we've sucessfully sent a message, model.myConversation should be valid
        showConversationMessageSentToast(model.myConversation, message);
      });

      $mdDialog.hide();
    };
    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

  }

  function showConversationMessageSentToast(conversation, message) {
    $mdToast.show(
      $mdToast.simple()
        .content('Message sent!')
        .hideDelay(3000)
    );
  };
  this.showConversationMessageSentToast = showConversationMessageSentToast;




}]);
