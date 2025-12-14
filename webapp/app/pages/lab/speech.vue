<script setup lang="ts">
import * as SpeechSDK from 'microsoft-cognitiveservices-speech-sdk';

const isLoading = ref(false);
const isSpeaking = ref(false);
const statusMessage = ref('Ready to synthesize.');
const textToSpeak = ref('Hello! The secure connection to Azure Speech is working perfectly.');

// 1. Fetch the Token from YOUR Backend
async function getSpeechToken() {
    try {
        // This hits your C# endpoint: app.MapGet("/speech/token", ...)
        // Nuxt's $fetch automatically includes your auth cookies if configured correctly
        const data = await $fetch<{ token: string; region: string }>('/api/speech/token');

        return {
            authToken: data.token,
            region: data.region
        };
    } catch (err) {
        const errorMessage = err instanceof Error ? err.message : String(err);
        statusMessage.value = `Token Fetch Failed: ${errorMessage}`;
        console.error(err);
        return null;
    }
}

// 2. The Main Action
async function speak() {
    if (isSpeaking.value) return;

    isLoading.value = true;
    statusMessage.value = 'Authenticating...';

    // A. Get the secure credentials
    const speechConfigData = await getSpeechToken();

    if (!speechConfigData) {
        isLoading.value = false;
        return;
    }

    // B. Configure the Azure SDK locally
    const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
        speechConfigData.authToken,
        speechConfigData.region
    );

    // Optional: Set a specific voice (e.g., Jenny is very popular/realistic)
    speechConfig.speechSynthesisVoiceName = "en-US-JennyNeural";

    const audioConfig = SpeechSDK.AudioConfig.fromDefaultSpeakerOutput();
    const synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig, audioConfig);

    // C. Speak
    statusMessage.value = 'Synthesizing audio...';

    synthesizer.speakTextAsync(
        textToSpeak.value,
        (result) => {
            // Success or Failure callback
            if (result.reason === SpeechSDK.ResultReason.SynthesizingAudioCompleted) {
                statusMessage.value = 'Speech synthesis complete.';
            } else {
                statusMessage.value = 'Speech canceled or failed. Check console.';
                console.error(result.errorDetails);
            }
            synthesizer.close(); // Clean up
            isLoading.value = false;
        },
        (err) => {
            statusMessage.value = `Error: ${err}`;
            synthesizer.close();
            isLoading.value = false;
        }
    );
}
</script>

<template>
    <div class="p-6 max-w-2xl mx-auto">
        <div class="mb-6">
            <h1 class="text-2xl font-bold mb-2">Neural Voice Lab</h1>
            <p class="text-gray-600">
                This interface tests the full loop:
                <span class="font-mono text-xs bg-gray-200 px-1 rounded">UI</span> ->
                <span class="font-mono text-xs bg-gray-200 px-1 rounded">.NET API (Auth)</span> ->
                <span class="font-mono text-xs bg-gray-200 px-1 rounded">Azure (Token)</span> ->
                <span class="font-mono text-xs bg-gray-200 px-1 rounded">Browser (Audio)</span>
            </p>
        </div>

        <div class="bg-white rounded-lg shadow p-6 border border-gray-200">
            <label class="block text-sm font-medium text-gray-700 mb-2">Text to Synthesize</label>
            <textarea v-model="textToSpeak" rows="4"
                class="w-full p-3 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
                placeholder="Type something here to verify the voice output..."></textarea>

            <div class="mt-4 flex items-center justify-between">
                <span class="text-sm" :class="statusMessage.includes('Error') ? 'text-red-600' : 'text-gray-500'">
                    Status: {{ statusMessage }}
                </span>

                <button @click="speak" :disabled="isLoading || !textToSpeak"
                    class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2 transition">
                    <span v-if="isLoading" class="animate-spin">⏳</span>
                    <span v-else>🔊</span>
                    {{ isLoading ? 'Processing...' : 'Speak Text' }}
                </button>
            </div>
        </div>

        <div class="mt-6">
            <NuxtLink to="/lab" class="text-blue-600 hover:underline">← Back to Lab Dashboard</NuxtLink>
        </div>
    </div>
</template>