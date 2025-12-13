<script setup>
  import { ref } from 'vue'

  const open = ref(false)

  // Auth Logic
  const { user, isAuthenticated, login, logout, fetchUser } = useAuth()
  await fetchUser()

  const navItems = [
    { title: 'Home', to: '/', icon: 'mdi-home' },
    { title: 'Projects', to: '/projects', icon: 'mdi-folder-star' },
    { title: 'Blog', to: '/blog', icon: 'mdi-post-outline' },
    { title: 'AI Experiments', to: '/lab', icon: 'mdi-flask' },
  ]
  
  // Footer Links Data
  const socialLinks = [
    { icon: 'mdi-github', url: 'https://github.com/amadorcarlosa', title: 'GitHub' },
    { icon: 'mdi-linkedin', url: 'https://linkedin.com/in/caamador', title: 'LinkedIn' },
    { icon: 'mdi-sigma', url: 'https://mathtabla.com', title: 'MathTabla' } // Using Sigma icon for Math
  ]
</script>

<template>
  <v-app>
    <v-app-bar elevation="1">
      <v-btn icon="mdi-menu" variant="text" @click="open = !open"></v-btn>
      <v-toolbar-title class="text-h6">
        <span class="font-weight-black text-uppercase">Carlos Amador's</span>
        <span class="font-weight-light text-primary ml-1">Lab</span>
      </v-toolbar-title>
      <v-spacer />
      <ClientOnly>
        <template v-if="isAuthenticated">
          <div class="d-flex align-center">
            <span class="text-caption mr-2 d-none d-sm-block">{{ user?.name || 'Admin' }}</span>
            <v-menu>
              <template v-slot:activator="{ props }">
                <v-btn icon v-bind="props">
                  <v-avatar color="secondary" size="32">
                    <span class="text-caption font-weight-bold">AC</span>
                  </v-avatar>
                </v-btn>
              </template>
              <v-list>
                <v-list-item title="Profile" prepend-icon="mdi-account" to="/lab/profile" />
                <v-divider class="my-1" />
                <v-list-item title="Logout" prepend-icon="mdi-logout" @click="logout()" />
              </v-list>
            </v-menu>
          </div>
        </template>
        <template v-else>
          <v-btn prepend-icon="mdi-login" variant="tonal" class="ml-2 mr-2" @click="login()">Log In</v-btn>
        </template>
      </ClientOnly>
    </v-app-bar>

    <v-navigation-drawer v-model="open">
      <v-list>
        <v-list-item v-for="item in navItems" :key="item.title" :title="item.title" :to="item.to" :prepend-icon="item.icon" />
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <slot />
    </v-main>

    <v-footer app border class="bg-grey-lighten-4 d-flex flex-column py-6">
      
      <div class="d-flex gap-4 mb-4">
        <v-btn 
          v-for="link in socialLinks" 
          :key="link.title"
          :icon="link.icon"
          :href="link.url"
          target="_blank"
          variant="text"
          color="grey-darken-2"
          size="large"
          :title="link.title"
        />
      </div>

      <div class="text-center text-caption text-grey-darken-1">
        <div>&copy; {{ new Date().getFullYear() }} Carlos Amador. All rights reserved.</div>
        <div class="mt-1">
          Built with <strong>Nuxt 4</strong>, <strong>.NET Aspire</strong>, and <strong>Azure</strong>.
        </div>
      </div>
    </v-footer>
  </v-app>
</template>