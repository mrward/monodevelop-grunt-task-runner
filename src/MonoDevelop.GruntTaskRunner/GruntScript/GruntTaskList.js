module.exports = function(grunt) {
  // Writes out tasks found by Grunt in json format.
  grunt.task.registerTask('md-grunt-task-list', 'Lists tasks', function() {
    grunt.log.writeln(JSON.stringify(grunt.task._tasks));
  });
};
