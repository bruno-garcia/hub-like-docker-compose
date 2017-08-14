export const environment = {
  production: false,
  // can't access 'process.env' here: https://github.com/angular/angular-cli/issues/4318
  // Whatever hostname the browser testing this instance can reach
  apiUrl: 'http://athens.lan:5000'
};
