<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import * as SpeechSDK from 'microsoft-cognitiveservices-speech-sdk'

// Config
const TARGET_LOCALE = 'es-MX'
// We use a complex sentence to test words, syllables, and punctuation
const REFERENCE_TEXT = "Mi mamá me ama."

// State
const status = ref('idle')
const rawJson = ref<any>(null)
const errorMsg = ref('')

let recognizer: SpeechSDK.SpeechRecognizer | null = null

const runInspection = async () => {
    try {
        status.value = 'initializing'
        rawJson.value = null
        errorMsg.value = ''

        // 1. Get Token
        const { data: tokenData } = await useAuthFetch<{ token: string; region: string }>('/api/speech/token')

        if (!tokenData.value) {
            throw new Error('Failed to retrieve speech token')
        }

        // 2. Setup Azure
        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
            tokenData.value.token,
            tokenData.value.region
        )
        speechConfig.speechRecognitionLanguage = TARGET_LOCALE

        // 3. Pronunciation Config (With Mis-cue enabled)
        const pronunciationConfig = new SpeechSDK.PronunciationAssessmentConfig(
            REFERENCE_TEXT,
            SpeechSDK.PronunciationAssessmentGradingSystem.HundredMark,
            SpeechSDK.PronunciationAssessmentGranularity.Phoneme, // Detailed view
            true
        )

        const audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput()
        recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig)
        pronunciationConfig.applyTo(recognizer)

        status.value = 'listening'

        // 4. Capture ONE shot
        recognizer.recognizeOnceAsync(
            (result) => {
                // Success or Fail, we want to see what happened
                status.value = 'idle'

                // Extract the hidden JSON blob
                const jsonString = result.properties.getProperty(
                    SpeechSDK.PropertyId.SpeechServiceResponse_JsonResult
                )

                if (jsonString) {
                    rawJson.value = JSON.parse(jsonString)
                    console.log("Full Azure Response:", rawJson.value) // Also log to console
                } else {
                    errorMsg.value = "No JSON received. Did you speak?"
                }

                recognizer?.close()
                recognizer = null
            },
            (err) => {
                errorMsg.value = `SDK Error: ${err}`
                status.value = 'error'
                recognizer?.close()
            }
        )

    } catch (err: any) {
        errorMsg.value = err.message
        status.value = 'error'
    }
}

onUnmounted(() => {
    recognizer?.close()
})
</script>

<template>
    <v-container>
        <h1 class="text-h4 font-weight-bold mb-4">Azure JSON Inspector</h1>
        <p class="mb-4">
            Reference Text: <v-chip>{{ REFERENCE_TEXT }}</v-chip>
        </p>

        <v-btn color="primary" size="large" @click="runInspection" :loading="status === 'listening'"
            :disabled="status === 'listening'">
            {{ status === 'listening' ? 'Listening...' : 'Record & Inspect' }}
        </v-btn>

        <v-alert v-if="errorMsg" type="error" class="mt-4">{{ errorMsg }}</v-alert>

        <v-card v-if="rawJson" class="mt-6 border" elevation="0">
            <v-card-title class="bg-grey-lighten-4">Raw API Response</v-card-title>
            <v-card-text class="pa-0">
                <pre class="code-block">{{ JSON.stringify(rawJson, null, 2) }}</pre>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<style scoped>
.code-block {
    background-color: #1e1e1e;
    color: #d4d4d4;
    padding: 20px;
    overflow-x: auto;
    font-family: 'Consolas', 'Monaco', monospace;
    font-size: 14px;
    max-height: 600px;
}
</style>