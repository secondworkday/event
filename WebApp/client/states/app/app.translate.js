app.config(['$translateProvider', function ($translateProvider) {
  $translateProvider.translations('demo', {
    'USER': 'User',
    'USERS': 'Users',

    'EVENT': 'Event',
    'EVENTS': 'Events',
    'EVENT_SESSION': 'Session',
    'EVENT_SESSIONS': 'Sessions',
    'PARTICIPANT': 'Participant',
    'PARTICIPANTS': 'Participants',
    'PARTICIPANT_GROUP': 'Participant Group',
    'PARTICIPANT_GROUPS': 'Participant Groups',
    'LOCATION': 'Location',
    'LOCATIONS': 'Locations',

    'TENANT_ADMIN_ROLE': 'Admin',
    'TENANT_ADMINS_ROLE': 'Admins',
    'EVENT_PLANNER_ROLE': 'Event Planner',
    'EVENT_PLANNERS_ROLE': 'Event Planners',
    'SESSION_MANAGER': 'Session Manager',
    'SESSION_MANAGERS': 'Session Managers',
    'SESSION_VOLUNTEER_ROLE': 'Session Volunteer',
    'SESSION_VOLUNTEERS_ROLE': 'Session Volunteers',
    'PARTICIPANT_GROUP_CONTACT_ROLE': 'Participant Group Contact',
    'PARTICIPANT_GROUP_CONTACTS_ROLE': 'Participant Group Contacts'
});

  $translateProvider.translations('soccer', {
    'USER': 'Staff',
    'USERS': 'Staff',

    'EVENT': 'Jamboree',
    'EVENTS': 'Jamborees',
    'EVENT_SESSION': 'Skills Clinic',
    'EVENT_SESSIONS': 'Skills Clinics',
    'PARTICIPANT': 'Player',
    'PARTICIPANTS': 'Players',
    'PARTICIPANT_GROUP': 'Club',
    'PARTICIPANT_GROUPS': 'Clubs',
    'LOCATION': 'Field',
    'LOCATIONS': 'Fields',

    'TENANT_ADMIN_ROLE': 'Admin',
    'TENANT_ADMINS_ROLE': 'Admins',
    'EVENT_PLANNER_ROLE': 'Coach',
    'EVENT_PLANNERS_ROLE': 'Coaches',
    'SESSION_MANAGER': 'Head Coach',
    'SESSION_MANAGERS': 'Head Coaches',
    'SESSION_VOLUNTEER_ROLE': 'Assistant',
    'SESSION_VOLUNTEERS_ROLE': 'Assistants',
    'PARTICIPANT_GROUP_CONTACT_ROLE': 'Club Manager',
    'PARTICIPANT_GROUP_CONTACTS_ROLE': 'Club Managers'
});


  $translateProvider.translations('ale', {
    'USER': 'Team Member',
    'USERS': 'Team',

    'EVENT': 'Session',
    'EVENTS': 'Sessions',
    'EVENT_SESSION': 'Shopping Event',
    'EVENT_SESSIONS': 'Shopping Events',
    'PARTICIPANT': 'Student',
    'PARTICIPANTS': 'Students',
    'PARTICIPANT_GROUP': 'School Contact',
    'PARTICIPANT_GROUPS': 'School Contacts', // issue: concepts are now a mismatch, name changed per Bob's request
    'LOCATION': 'Store',
    'LOCATIONS': 'Stores',

    'TENANT_ADMIN_ROLE': 'Admin',
    'TENANT_ADMINS_ROLE': 'Admins',
    'EVENT_PLANNER_ROLE': 'Event Planner',
    'EVENT_PLANNERS_ROLE': 'Event Planners',
    'SESSION_MANAGER': 'Store Leader',
    'SESSION_MANAGERS': 'Store Leaders',
    'SESSION_VOLUNTEER_ROLE': 'Check-in Volunteer',
    'SESSION_VOLUNTEERS_ROLE': 'Check-in Volunteers',
    'PARTICIPANT_GROUP_CONTACT_ROLE': 'School Contact',
    'PARTICIPANT_GROUP_CONTACTS_ROLE': 'School Contacts'
});

  $translateProvider.preferredLanguage('ale');
}]);
