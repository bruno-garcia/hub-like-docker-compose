FROM node:6.11.1 AS ng-build
WORKDIR /app

ADD package.json ./
RUN npm install

COPY . ./
# angualr cli doesn't support process.env for its config system: https://github.com/angular/angular-cli/issues/4318
RUN node node_modules/@angular/cli/bin/ng build --environment=docker

FROM nginx

WORKDIR /usr/share/nginx/html
COPY --from=ng-build /app/dist/. ./
EXPOSE 80
