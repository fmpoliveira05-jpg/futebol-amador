import { defineConfig } from 'cypress'

export default defineConfig({
  
  e2e: {
    setupNodeEvents(on, config) {},
    video: true,
    screenshotsFolder: "cypress/screenshots",
    videosFolder: "cypress/videos",
    'baseUrl': 'http://localhost:4200'
  },
  
  
})
