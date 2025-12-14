<script setup lang="ts">
import * as SpeechSDK from 'microsoft-cognitiveservices-speech-sdk';

// 1. Protect this route (just like your Weather page)
definePageMeta({
  middleware: 'auth'
})

const isLoading = ref(false);
const isSpeaking = ref(false);
const statusMessage = ref('Ready to synthesize.');
const textToSpeak = ref('Hello! The secure connection to Azure Speech is working perfectly.');
const isError = ref(false); // Track if status is an error for red alert styling

// 2. Fetch the Token (Logic remains the same, just cleaner)
async function getSpeechToken() {
  try {
    const data = await $fetch<{ token: string; region: string }>('/api/speech/token');
    return {
      authToken: data.token,
      region: data.region
    };
  } catch (err: any) {
    const msg = err.message || String(err);
    statusMessage.value = `Token Fetch Failed: ${msg}`;
    isError.value = true;
    console.error(err);
    return null;
  }
}

// 3. The Main Action
async function speak() {
  if (isSpeaking.value) return;
  
  isLoading.value = true;
  isError.value = false;
  statusMessage.value = 'Authenticating with Azure...';

  // A. Get credentials
  const speechConfigData = await getSpeechToken();
  
  if (!speechConfigData) {
    isLoading.value = false;
    return; // Error state is already set in getSpeechToken
  }

  // B. Configure SDK
  const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
    speechConfigData.authToken, 
    speechConfigData.region
  );
  speechConfig.speechSynthesisVoiceName = "en-US-JennyNeural"; 

  const audioConfig = SpeechSDK.AudioConfig.fromDefaultSpeakerOutput();
  const synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig, audioConfig);

  // C. Speak
  statusMessage.value = 'Synthesizing audio stream...';
  
  synthesizer.speakTextAsync(
    textToSpeak.value,
    (result) => {
      if (result.reason === SpeechSDK.ResultReason.SynthesizingAudioCompleted) {
        statusMessage.value = 'Speech synthesis complete.';
        isError.value = false;
      } else {
        statusMessage.value = 'Speech canceled or failed. Check console.';
        isError.value = true;
        console.error(result.errorDetails);
      }
      synthesizer.close();
      isLoading.value = false;
    },
    (err) => {
      statusMessage.value = `Error: ${err}`;
      isError.value = true;
      synthesizer.close();
      isLoading.value = false;
    }
  );
}
</script>

<template>
  <v-container>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold">Neural Voice Lab</h1>
      <p class="text-body-1 text-medium-emphasis">
        This interface tests the full loop: 
        <v-chip size="x-small" label class="mx-1">UI</v-chip> → 
        <v-chip size="x-small" label class="mx-1">.NET API</v-chip> → 
        <v-chip size="x-small" label class="mx-1">Azure Token</v-chip> → 
        <v-chip size="x-small" label class="mx-1">Browser Audio</v-chip>
      </p>
    </div>

    <v-card border elevation="0" class="pa-4">
      
      <v-textarea
        v-model="textToSpeak"
        label="Text to Synthesize"
        variant="outlined"
        rows="3"
        color="primary"
        hide-details="auto"
        placeholder="Type something here to verify the voice output..."
        class="mb-4"
      ></v-textarea>

      <div class="d-flex align-center justify-space-between">
        <div class="d-flex align-center text-caption text-medium-emphasis">
            <v-icon 
              v-if="isError" 
              icon="mdi-alert-circle" 
              color="error" 
              size="small" 
              class="mr-2"
            />
            <v-icon 
              v-else-if="isLoading" 
              icon="mdi-loading" 
              class="mdi-spin mr-2" 
              size="small" 
              color="primary"
            />
            <v-icon 
              v-else 
              icon="mdi-check-circle" 
              color="success" 
              size="small" 
              class="mr-2"
            />
            {{ statusMessage }}
        </div>

        <v-btn
          color="primary"
          prepend-icon="mdi-volume-high"
          :loading="isLoading"
          :disabled="!textToSpeak"
          @click="speak"
        >
          Speak Text
        </v-btn>
      </div>

      <v-expand-transition>
        <div v-if="isError" class="mt-4">
            <v-alert
            type="error"
            variant="tonal"
            density="compact"
            title="Synthesis Error"
            :text="statusMessage"
            ></v-alert>
        </div>
      </v-expand-transition>

    </v-card>

    <div class="mt-6">
      <v-btn variant="text" prepend-icon="mdi-arrow-left" to="/lab" color="primary">
        Back to Lab Dashboard
      </v-btn>
    </div>

  </v-container>
</template>