<template>
  <v-container class="fill-height justify-center">
    <v-card max-width="500" class="text-center pa-8" elevation="4" rounded="xl">
      <v-icon icon="mdi-shield-lock-outline" size="100" color="red-lighten-2" class="mb-4" />
      
      <h1 class="text-h4 font-weight-bold mb-2">Access Denied</h1>
      <p class="text-body-1 text-grey mb-6">
        This laboratory protocol requires active authentication credentials.
      </p>

      <div class="d-flex justify-center gap-4">
        <v-btn variant="text" to="/">Return Home</v-btn>
        <v-btn color="primary" @click="useAuth().login()">Log In</v-btn>
      </div>
    </v-card>
  </v-container>
</template>
