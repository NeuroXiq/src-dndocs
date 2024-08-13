// need to be set on build/by cmd or powershell
// like (powershell): 
// >
// > $Env:NEXTJS_ROBINIADOCS_ENV='Staging'
//

const env = process.env.NEXTJS_APPINFO_ENV || 'Development';
const versionInfo = process.env.NEXTJS_APPINFO_VERSION || '0.0.0.0-Development';
// maybe move client_id=... to backend and fetch on login???
const current = {
  Development: {
    backendUrl: 'https://localhost:7111/',
    githubOAuthLoginUrl: 'https://github.com/login/oauth/authorize?scope=user%3Aemail&client_id=51459d3ec22cb7d0a88d',
  },
  Staging: {
    backendUrl: 'https://staging.dndocs.com/',
    githubOAuthLoginUrl: 'https://github.com/login/oauth/authorize?scope=user%3Aemail&client_id=2e155d9852729f1a6cdb',

  },
  Production: {
    backendUrl: 'https://dndocs.com/',
    githubOAuthLoginUrl: 'https://github.com/login/oauth/authorize?scope=user%3Aemail&client_id=220f765d5245b029ede7',
  },
}[env];

const config = {
  versionInfo: versionInfo,
  backendUrl: current?.backendUrl,
  githubOAuthLoginUrl: current?.githubOAuthLoginUrl, 
  env: {
    isDevelopment: env === 'Development',
    isStaging: env === 'Staging',
    isProduction: env === 'Production',
  },
}

export default config;
