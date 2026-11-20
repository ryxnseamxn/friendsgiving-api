import { defineRailway, github, postgres, preserve, project, service } from 'railway/iac';

// The GitHub repo Railway deploys the API from. Pushes to its default branch redeploy.
const API_REPO = 'YOUR_GITHUB_USER/friendsgiving-api';

export default defineRailway(() => {
  // Managed Postgres; Railway attaches and backs its volume, so none is declared here.
  const db = postgres('postgres');

  const api = service('api', {
    source: github(API_REPO),
    build: { builder: 'DOCKERFILE', dockerfilePath: 'Dockerfile' },
    healthcheck: '/health',
    healthcheckTimeout: 60,
    deploy: { restartPolicyType: 'ON_FAILURE', restartPolicyMaxRetries: 10 },
    env: {
      // Npgsql can't parse postgres:// URLs; the API converts this one on startup.
      ConnectionStrings__DefaultConnection: db.env.DATABASE_URL,
      // Secret, so it stays out of git: set once with `railway variables --set Admin__ApiKey=...`.
      Admin__ApiKey: preserve(),
    },
  });

  return project('friendsgiving', { resources: [db, api] });
});
