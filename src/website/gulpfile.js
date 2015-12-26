/// <binding BeforeBuild='default' Clean='clean' ProjectOpened='default' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

var paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/site.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
  return gulp.src([paths.js, paths.webroot + "js/modules/*.js", "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:sar-database", function () {
  return gulp.src([paths.webroot + "js/models/**/*.js", paths.webroot + "js/directives/**/*.js", paths.webroot + "js/services/**/*.js", "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.webroot + "js/sar-database.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("."));
})

gulp.task("sar-map:js", function () {
  return gulp.src([paths.webroot + "js/lib/mapbox.js/mapbox.js", paths.webroot + "js/lib/leaflet.google.js", paths.webroot + "js/lib/TileLayer.GeoJSON.js", paths.webroot + "js/maps/map.js"])
        .pipe(concat(paths.webroot + "js/sar-maps.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("."))
})

gulp.task("sar-map:css", function () {
  return gulp.src([paths.webroot + "css/site-mapbox.css", "!" + paths.minCss])
      .pipe(concat(paths.webroot + "css/sar-map.min.css"))
      .pipe(cssmin())
      .pipe(gulp.dest("."));
})

gulp.task("sar-map", ["sar-map:js", "sar-map:css"])

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:sar-database", "min:css"]);

gulp.task("default", ["min", "sar-map"])
