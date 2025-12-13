<template>
  <v-container class="py-12">
    <v-row align="center" class="mb-16">
      <v-col cols="12" md="8">
        <div class="text-overline text-primary font-weight-bold mb-2">
          Full-Stack .NET Architect
        </div>
        <h1 class="text-h2 font-weight-black text-blue-grey-darken-4 mb-4">
          Building Event-Driven,<br>
          <span class="text-primary">Cognitive Systems.</span>
        </h1>
        <p class="text-h6 font-weight-light text-medium-emphasis mb-6" style="max-width: 600px; line-height: 1.6;">
          I orchestrate distributed cloud applications using <strong>.NET Aspire</strong> and <strong>Azure</strong>. 
          By combining <strong>Cognitive Science</strong> principles with modern architecture, 
          I build software that is adaptive, resilient, and intelligent.
        </p>
        
        <div class="d-flex gap-4">
          <v-btn color="primary" size="x-large" to="/projects" flat>
            View Projects
          </v-btn>
          <v-btn variant="outlined" size="x-large" to="/lab">
            Enter the Lab
            <v-icon end icon="mdi-arrow-right" />
          </v-btn>
        </div>
      </v-col>
      
      <v-col cols="12" md="4" class="d-none d-md-block">
        <v-card class="bg-blue-grey-darken-4 text-code" elevation="10" border>
          <v-card-text class="font-mono text-caption">
            <div class="text-green-accent-3">// Architecture.cs</div>
            <div class="mt-2"><span class="text-purple-lighten-2">var</span> app = DistributedApplication.CreateBuilder(args);</div>
            <div class="mt-2">
              <span class="text-grey">var</span> api = app.AddProject&lt;Projects.Api&gt;(<span class="text-amber">"api"</span>);<br>
              <span class="text-grey">var</span> serviceBus = app.AddAzureServiceBus(<span class="text-amber">"messaging"</span>);
            </div>
            <div class="mt-2 text-blue-lighten-3">app.Build().Run();</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mb-16">
      <v-col cols="12" md="4">
        <v-card height="100%" hover border>
          <v-card-item>
            <template v-slot:prepend>
              <v-icon color="primary" size="x-large" icon="mdi-cloud-braces" />
            </template>
            <v-card-title>Orchestration</v-card-title>
          </v-card-item>
          <v-card-text>
            Scaling complexity with <strong>.NET Aspire</strong> and <strong>Azure Container Apps</strong>. 
            Designing event-driven backends with Azure Functions and Service Bus for 99.9% uptime.
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card height="100%" hover border>
          <v-card-item>
            <template v-slot:prepend>
              <v-icon color="purple" size="x-large" icon="mdi-brain" />
            </template>
            <v-card-title>Intelligence</v-card-title>
          </v-card-item>
          <v-card-text>
            Integrating <strong>Semantic Kernel</strong> and <strong>MCP Servers</strong> to build context-aware AI agents. 
            Leveraging my M.A. in Neuroscience to design adaptive user experiences.
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card height="100%" hover border>
          <v-card-item>
            <template v-slot:prepend>
              <v-icon color="green" size="x-large" icon="mdi-monitor-dashboard" />
            </template>
            <v-card-title>Interface</v-card-title>
          </v-card-item>
          <v-card-text>
            Building real-time dashboards with <strong>Nuxt 4</strong>, <strong>SignalR</strong>, and Pinia. 
            Creating secure BFF patterns for seamless OIDC/Auth integration.
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="align-center mb-12 bg-grey-lighten-4 rounded-xl pa-8 mx-0">
      <v-col cols="12" md="6">
        <div class="text-caption text-uppercase font-weight-bold mb-2">
          Production Case Study
        </div>
        <h2 class="text-h4 font-weight-bold mb-4">MathTabla Platform</h2>
        <p class="mb-4">
          A multi-tenant SaaS platform serving thousands of students. 
          Architected on Azure using a <strong>.NET 9 microservices</strong> pattern.
        </p>
        <v-list density="compact" bg-color="transparent">
          <v-list-item prepend-icon="mdi-check-circle" title="Automated Grading via Google Classroom API" />
          <v-list-item prepend-icon="mdi-check-circle" title="Real-time Collaboration via SignalR" />
          <v-list-item prepend-icon="mdi-check-circle" title="CI/CD via GitHub Actions -> Container Apps" />
        </v-list>
        <v-btn class="mt-4" variant="text" color="primary" href="https://mathtabla.com" target="_blank">
          Visit Live Production Site <v-icon end icon="mdi-open-in-new" />
        </v-btn>
      </v-col>
      
      <v-col cols="12" md="6">
         <v-sheet color="white" height="300" class="d-flex align-center justify-center rounded-lg border">
           <div class="text-center">
             <v-icon icon="mdi-layers-triple" size="64" color="grey-lighten-2" />
             <div class="text-caption text-grey mt-2">System Architecture Diagram</div>
           </div>
         </v-sheet>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.font-mono {
  font-family: 'Consolas', 'Monaco', monospace;
}
</style>

