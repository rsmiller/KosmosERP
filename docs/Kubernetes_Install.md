
# Kosmos ERP - Kubernetes Install

This is a install document related to Kubernetes or Rancher.

## Installation
In the Kubernetes folder of this project has all the scripts required for an install including Superset, if you choose to use it.

### Step #1
In the Secrets.yaml there are several sections that require a username and password. You will need to examine these secret definitions and set your username, password, and database connection string.

 - [ ] kosmos-erp-db-secret
 - [ ] kosmos-erp-hangfire-secret
 - [ ] kosmos-erp-api-secret
 - [ ] kosmos-erp-stripe-secret
 - [ ] kosmos-erp-keycloak-secret

If using Apache Superset for reports:
 - [ ] kosmos-erp-superset-secret
 - [ ] kosmos-erp-redis-secret

If you are using a private repositoy:
 - [ ] gitlab-registry-secret

## Step #2
The bulk of the configuration will be in the api-deployment.yaml file. This is where you will set how you will be using message publishing, where and how files are stored, logging provider, and Keycloak information. There are a few architectual and design consideration you will need to figure out before implementing. These desisions are:

 - [ ] Message Publisher: database, azure, rabbitmq
 - [ ] Payment Provider: stripe
 - [ ] Log Provider: azure, datadog, database
 - [ ] Storage Type: local, azure, aws
 - [ ] Authentication Provder: keycloak

Depending on what you choose there will be additional environmental variables that will need to be set.
