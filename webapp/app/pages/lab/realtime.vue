<script setup lang="ts">
// Intentionally empty.
// This page exists to communicate intent, not execution.
</script>
<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-row justify="center">
      <v-col cols="12" md="8" lg="6">
        <v-card
          elevation="8"
          rounded="xl"
          class="pa-6"
        >
          <v-card-title class="d-flex align-center mb-2">
            <v-icon
              icon="mdi-flash"
              size="36"
              color="amber"
              class="me-3"
            />
            <div>
              <div class="text-h5 font-weight-bold">
                Real-Time Event Bus
              </div>
              <div class="text-subtitle-2 text-medium-emphasis">
                SignalR & Azure Service Bus
              </div>
            </div>
          </v-card-title>

          <v-divider class="my-4" />

          <v-alert
            type="warning"
            variant="tonal"
            border="start"
            color="amber"
            icon="mdi-progress-wrench"
            class="mb-4"
          >
            This lab is currently <strong>planned</strong> and under active design.
            The system wiring is not yet live.
          </v-alert>

          <v-card-text class="text-body-1">
            This page will host a live visualization of infrastructure events flowing
            through the platform in real time.
            <br /><br />
            When complete, it will:
            <ul class="mt-2">
              <li>Stream events from <strong>Azure Service Bus</strong></li>
              <li>Relay them via <strong>SignalR</strong></li>
              <li>Render a real-time “heartbeat” of system activity</li>
            </ul>
            <br />
            Think of it as an observatory: not business logic, not UI polish,
            but raw system motion made visible.
          </v-card-text>

          <v-divider class="my-4" />

          <v-chip
            color="amber"
            variant="outlined"
            prepend-icon="mdi-timeline-clock-outline"
          >
            Status: Planned
          </v-chip>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>


<style scoped>


</style>
    description: 'Syllable-level pronunciation grader for Spanish literacy. Listens to mic input and 