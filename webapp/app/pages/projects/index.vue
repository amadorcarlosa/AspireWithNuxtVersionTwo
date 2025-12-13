<template>
  <v-container class="py-12">
    <v-row class="mb-12">
      <v-col cols="12" md="8">
        <h1 class="text-h3 font-weight-black mb-2">Engineering Portfolio</h1>
        <p class="text-h6 text-medium-emphasis font-weight-light">
          Production systems, cloud architecture, and cognitive science research.
        </p>
      </v-col>
      <v-col cols="12" md="4" class="text-md-right">
        <div class="d-flex flex-column align-md-end gap-2">
          <div class="d-flex align-center text-caption text-grey">
            <v-icon icon="mdi-map-marker" size="small" class="mr-1" />
            Little Elm / Dallas, TX
          </div>
          <div class="d-flex gap-2 mt-2">
            <v-btn icon="mdi-github" variant="text" href="https://github.com/amadorcarlosa" target="_blank" title="GitHub" />
            <v-btn icon="mdi-linkedin" variant="text" href="https://linkedin.com/in/caamador" target="_blank" title="LinkedIn" />
            <v-btn icon="mdi-school" variant="text" href="https://orcid.org/0009-0009-5901-7708" target="_blank" title="ORCID Research" />
          </div>
        </div>
      </v-col>
    </v-row>

    <v-row class="mb-16">
      <v-col cols="12">
        <div class="text-overline text-primary font-weight-bold mb-2">Flagship Production Platform</div>
        <v-card class="rounded-xl overflow-hidden" elevation="4" border>
          <v-row no-gutters>
            <v-col cols="12" md="6" class="bg-blue-grey-darken-4 d-flex align-center justify-center pa-8">
              <div class="text-center">
                <v-icon icon="mdi-sigma" size="80" class="mb-4 text-white" />
                <h3 class="text-h4 font-weight-bold text-white">MathTabla</h3>
                <div class="text-caption text-blue-grey-lighten-3 mt-2">SaaS Education Platform</div>
              </div>
            </v-col>
            
            <v-col cols="12" md="6" class="pa-8 pa-md-10">
              <div class="d-flex justify-space-between align-start">
                <h2 class="text-h4 font-weight-bold mb-4">MathTabla.com</h2>
                <v-chip color="success" size="small" variant="flat">Live Production</v-chip>
              </div>
              
              <p class="text-body-1 mb-6 text-grey-darken-1">
                A multi-tenant SaaS platform built on <strong>.NET Aspire</strong> and Azure. 
                It automates grading and feedback for thousands of students by integrating directly with 
                Google Classroom and Skyward SIS.
              </p>

              <div class="mb-6">
                <div class="text-caption font-weight-bold mb-2">CORE ARCHITECTURE</div>
                <v-chip-group>
                  <v-chip size="small" color="purple">.NET Aspire</v-chip>
                  <v-chip size="small" color="blue">Azure B2C</v-chip>
                  <v-chip size="small" color="orange">SignalR</v-chip>
                  <v-chip size="small" color="green">Vue.js</v-chip>
                  <v-chip size="small" color="cyan">Semantic Kernel</v-chip>
                </v-chip-group>
              </div>

              <v-btn 
                color="primary" 
                size="large" 
                prepend-icon="mdi-open-in-new" 
                href="https://mathtabla.com" 
                target="_blank"
              >
                Visit Platform
              </v-btn>
            </v-col>
          </v-row>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <h3 class="text-h5 font-weight-bold mb-6">Architecture & Research</h3>
      </v-col>

      <v-col cols="12" md="6">
        <v-card height="100%" hover border class="d-flex flex-column">
          <v-card-item>
            <template v-slot:prepend>
              <v-avatar color="grey-lighten-4" rounded>
                <v-icon icon="mdi-flask-outline" color="black" />
              </v-avatar>
            </template>
            <v-card-title>The Amador Platform</v-card-title>
            <v-card-subtitle>Cloud-Native Prototype Lab</v-card-subtitle>
          </v-card-item>
          
          <v-card-text class="flex-grow-1">
            <p class="mb-4">
              A distributed system for prototyping AI agents. Implements a <strong>BFF (Backend for Frontend)</strong> 
              pattern to secure OIDC tokens and proxy API requests to .NET microservices.
            </p>
            <v-chip size="x-small" variant="outlined" class="mr-1">Nuxt 4</v-chip>
            <v-chip size="x-small" variant="outlined" class="mr-1">YARP</v-chip>
            <v-chip size="x-small" variant="outlined">Redis</v-chip>
          </v-card-text>
          
          <v-divider />
          
          <v-card-actions>
            <v-btn variant="text" color="primary" to="/lab">
              Enter Lab
            </v-btn>
            <v-spacer />
            <v-btn icon="mdi-github" size="small" variant="text" href="https://github.com/amadorcarlosa" target="_blank" />
          </v-card-actions>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card height="100%" hover border class="d-flex flex-column">
          <v-card-item>
            <template v-slot:prepend>
              <v-avatar color="green-lighten-5" rounded>
                <v-icon icon="mdi-brain" color="green-darken-2" />
              </v-avatar>
            </template>
            <v-card-title>Cognitive Science Research</v-card-title>
            <v-card-subtitle>M.A. Mind, Brain, and Education</v-card-subtitle>
          </v-card-item>
          
          <v-card-text class="flex-grow-1">
            <p class="mb-4">
              Research focusing on working memory and adaptive learning systems. 
              Applying neuroscience principles to software interface design and AI context management.
            </p>
            <div class="text-caption text-grey">
              <strong>Publications:</strong> "Working Memory and Teaching", "Fusion: Pedagogy, the Mind, and Brain"
            </div>
          </v-card-text>
          
          <v-divider />
          
          <v-card-actions>
            <v-btn variant="text" color="primary" href="https://orcid.org/0009-0009-5901-7708" target="_blank">
              View ORCID Profile
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>