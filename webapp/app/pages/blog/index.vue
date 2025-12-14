<template>
    <v-container class="py-8">
        <v-row class="mb-8">
            <v-col cols="12">
                <div class="d-flex align-center mb-2">
                    <v-icon icon="mdi-post-outline" color="primary" size="x-large" class="mr-3" />
                    <h1 class="text-h3 font-weight-black">Blog</h1>
                </div>
                <p class="text-h6 font-weight-light text-medium-emphasis" style="max-width: 800px;">
                    Thoughts on software architecture, cloud engineering, and lessons learned building modern
                    applications.
                </p>
            </v-col>
        </v-row>

        <v-row>
            <v-col cols="12">
                <v-alert type="info" variant="tonal" border="start" class="mb-6">
                    <v-alert-title>Coming Soon</v-alert-title>
                    Blog posts are being prepared. Check back soon for articles on .NET Aspire, Azure, and modern web
                    development.
                </v-alert>
            </v-col>
        </v-row>

        <v-row>
            <v-col cols="12" md="4">
                <v-card border class="h-100" variant="outlined">
                    <v-card-item>
                        <template #prepend>
                            <v-avatar color="primary" variant="tonal" rounded>
                                <v-icon icon="mdi-microsoft-azure" />
                            </v-avatar>
                        </template>
                        <v-card-title>Azure & Cloud</v-card-title>
                        <v-card-subtitle>Infrastructure & DevOps</v-card-subtitle>
                    </v-card-item>
                    <v-card-text class="text-medium-emphasis">
                        Deep dives into Azure services, managed identities, and cloud-native patterns.
                    </v-card-text>
                </v-card>
            </v-col>

            <v-col cols="12" md="4">
                <v-card border class="h-100" variant="outlined">
                    <v-card-item>
                        <template #prepend>
                            <v-avatar color="secondary" variant="tonal" rounded>
                                <v-icon icon="mdi-dot-net" />
                            </v-avatar>
                        </template>
                        <v-card-title>.NET Aspire</v-card-title>
                        <v-card-subtitle>Cloud-Native Development</v-card-subtitle>
                    </v-card-item>
                    <v-card-text class="text-medium-emphasis">
                        Building distributed applications with .NET Aspire orchestration.
                    </v-card-text>
                </v-card>
            </v-col>

            <v-col cols="12" md="4">
                <v-card border class="h-100" variant="outlined">
                    <v-card-item>
                        <template #prepend>
                            <v-avatar color="success" variant="tonal" rounded>
                                <v-icon icon="mdi-nuxt" />
                            </v-avatar>
                        </template>
                        <v-card-title>Frontend</v-card-title>
                        <v-card-subtitle>Nuxt, Vue & TypeScript</v-card-subtitle>
                    </v-card-item>
                    <v-card-text class="text-medium-emphasis">
                        Modern frontend patterns with Nuxt 4, Vue 3, and type-safe development.
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>
