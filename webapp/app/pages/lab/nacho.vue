<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import * as SpeechSDK from 'microsoft-cognitiveservices-speech-sdk'

// --- Configuration ---
const TARGET_LOCALE = 'es-MX'
const REFERENCE_TEXT = "ma me mi mo mu"
const TTS_VOICE = 'es-MX-DaliaNeural' // Warm female voice for teaching

// --- 1. STRICT TYPES ---
type AzurePronunciationAssessment = {
  AccuracyScore: number;
  ErrorType?: string;
}

type AzureSyllable = {
  Syllable: string;
  PronunciationAssessment?: AzurePronunciationAssessment; 
  Offset?: number;
  Duration?: number;
}

type AzureWord = {
  Word: string;
  ErrorType?: string;
  Syllables?: AzureSyllable[];
  PronunciationAssessment?: AzurePronunciationAssessment;
}

type AzureResult = {
  NBest?: { Words?: AzureWord[] }[];
}

// --- State ---
const status = ref<'idle' | 'initializing' | 'listening' | 'success' | 'error'>('idle')
const feedback = ref<{ text: string; score: number; status: string }[]>([])
const errorMsg = ref('')
const micVolume = ref(0)
const isPlayingAudio = ref(false) // Lock to prevent spamming clicks

// --- Azure Objects ---
let recognizer: SpeechSDK.SpeechRecognizer | null = null
let synthesizer: SpeechSDK.SpeechSynthesizer | null = null // <--- NEW: TTS Engine
let audioContext: AudioContext | null = null
let analyser: AnalyserNode | null = null
let microphoneStream: MediaStream | null = null
let visualizerFrame = 0

// Store token globally for this component so TTS can use it too
const currentAuth = ref<{ token: string; region: string } | null>(null)

// --- Helper: Spanish Syllable Splitter ---
const getExpectedSyllables = (text: string) => {
    return text.split(' ')
}

// --- 2. Visualizer ---
const initVisualizer = async () => {
    try {
        microphoneStream = await navigator.mediaDevices.getUserMedia({ audio: true })
        audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
        const source = audioContext.createMediaStreamSource(microphoneStream)
        analyser = audioContext.createAnalyser()
        analyser.fftSize = 256
        source.connect(analyser)
        drawVisualizer()
    } catch (e) {
        console.warn("Mic visualizer failed", e)
    }
}

const drawVisualizer = () => {
    if (!analyser) return
    const dataArray = new Uint8Array(analyser.frequencyBinCount)
    analyser.getByteFrequencyData(dataArray)
    const sum = dataArray.reduce((a, b) => a + b, 0)
    micVolume.value = sum / dataArray.length
    visualizerFrame = requestAnimationFrame(drawVisualizer)
}

// --- 3. Text-To-Speech (The New Feature) ---
const playSyllable = async (text: string) => {
    if (isPlayingAudio.value) return // Prevent overlap
    
    try {
        isPlayingAudio.value = true
        
        // 1. Ensure we have auth (Fetch if missing)
        if (!currentAuth.value) {
            const { data, error } = await useAuthFetch<{ token: string; region: string }>('/api/speech/token')
            if (error.value || !data.value) throw new Error("Could not authorize audio.")
            currentAuth.value = data.value
        }

        // 2. Config TTS
        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
            currentAuth.value.token, 
            currentAuth.value.region
        )
        speechConfig.speechSynthesisVoiceName = TTS_VOICE
        
        // 3. Create Synthesizer (disposable)
        synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig)

        // 4. Speak!
        synthesizer.speakTextAsync(
            text,
            result => {
                synthesizer?.close()
                synthesizer = null
                isPlayingAudio.value = false
            },
            err => {
                console.error(err)
                synthesizer?.close()
                isPlayingAudio.value = false
            }
        )

    } catch (e) {
        console.error("TTS Error", e)
        isPlayingAudio.value = false
    }
}

// --- 4. Azure Recognition Logic ---
const startSession = async () => {
    try {
        status.value = 'initializing'
        feedback.value = []
        errorMsg.value = ''

        await initVisualizer()

        // Fetch & Store Token
        const { data: tokenData, error: tokenError } = await useAuthFetch<{ token: string; region: string }>('/api/speech/token')
        if (tokenError.value || !tokenData.value) throw new Error('Backend authorization failed.')
        
        currentAuth.value = tokenData.value // Save for TTS later

        const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(
            tokenData.value.token,
            tokenData.value.region
        )
        speechConfig.speechRecognitionLanguage = TARGET_LOCALE
        speechConfig.setProperty(SpeechSDK.PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "5000")
        speechConfig.setProperty(SpeechSDK.PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "3000")

        const pronunciationConfig = new SpeechSDK.PronunciationAssessmentConfig(
            REFERENCE_TEXT,
            SpeechSDK.PronunciationAssessmentGradingSystem.HundredMark,
            SpeechSDK.PronunciationAssessmentGranularity.Phoneme,
            true
        )

        const audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput()
        recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig)
        pronunciationConfig.applyTo(recognizer)

        status.value = 'listening'

        recognizer.recognized = (s, e) => {
            if (e.result.reason === SpeechSDK.ResultReason.RecognizedSpeech) {
                processResult(e.result)
            } else if (e.result.reason === SpeechSDK.ResultReason.NoMatch) {
                errorMsg.value = "We didn't hear anything. Try moving closer."
                stopSession()
            }
        }

        recognizer.canceled = (s, e) => {
            if (e.reason === SpeechSDK.CancellationReason.Error) {
                errorMsg.value = `Connection Error: ${e.errorDetails}`
            }
            stopSession()
        }

        recognizer.startContinuousRecognitionAsync()

    } catch (err: any) {
        errorMsg.value = err.message
        status.value = 'error'
        stopSession(false)
    }
}

const stopSession = (resetStatus = true) => {
    if (resetStatus) status.value = 'idle'
    if (recognizer) {
        recognizer.stopContinuousRecognitionAsync(() => {
            recognizer?.close()
            recognizer = null
        })
    }
    if (audioContext) audioContext.close()
    if (microphoneStream) microphoneStream.getTracks().forEach(t => t.stop())
    cancelAnimationFrame(visualizerFrame)
    micVolume.value = 0
}

// --- 5. Parsing Logic ---
const processResult = (result: SpeechSDK.SpeechRecognitionResult) => {
    const jsonString = result.properties.getProperty(SpeechSDK.PropertyId.SpeechServiceResponse_JsonResult)
    const json: AzureResult = JSON.parse(jsonString)

    if (!json.NBest || !json.NBest[0] || !json.NBest[0].Words) {
        return
    }

    const azureWords = json.NBest[0].Words
    const expectedSyllables = getExpectedSyllables(REFERENCE_TEXT)

    const finalResults: { text: string; score: number; status: string }[] = []
    let flatAzureSyllables: { score: number; status: string }[] = []

    azureWords.forEach((word) => {
        if (word.ErrorType === "Omission") {
            flatAzureSyllables.push({ score: 0, status: 'skipped' })
            return
        }

        if (word.Syllables) {
            word.Syllables.forEach((syl) => {
                const rawScore = syl.PronunciationAssessment?.AccuracyScore ?? 0;
                flatAzureSyllables.push({
                    score: rawScore,
                    status: getScoreStatus(rawScore)
                })
            })
        }
    })

    expectedSyllables.forEach((text, index) => {
        const azureData = flatAzureSyllables[index] || { score: 0, status: 'skipped' }
        finalResults.push({
            text: text,
            score: azureData.score, 
            status: azureData.status
        })
    })

    feedback.value = finalResults
    status.value = 'success'
    stopSession(false)
}

const getScoreStatus = (score: number) => {
    if (score >= 80) return 'excellent'
    if (score >= 40) return 'good'
    return 'poor'
}

onUnmounted(() => {
    stopSession()
    synthesizer?.close()
})
</script>

<template>
    <v-container class="py-12 fill-height align-start">
        <v-row justify="center">
            <v-col cols="12" md="8" lg="6">

                <div class="text-center mb-8">
                    <div class="d-inline-block bg-pink-lighten-4 px-6 py-2 rounded-pill mb-4">
                        <h2 class="text-h4 font-weight-black text-pink-accent-3">
                            ma - me - mi
                        </h2>
                    </div>
                    <p class="text-body-1 text-grey-darken-1">
                        Lee las sílabas en voz alta. Haz clic para escuchar.
                    </p>
                </div>

                <v-card class="rounded-xl overflow-hidden border-0" elevation="10">

                    <div class="bg-yellow-lighten-5 pa-10 text-center position-relative" style="min-height: 250px;">

                        <div v-if="status !== 'success'" class="text-h3 font-weight-black text-blue-grey-darken-4"
                            style="letter-spacing: 2px;">
                            {{ REFERENCE_TEXT }}
                        </div>

                        <div v-else class="d-flex justify-center flex-wrap gap-3">
                            <v-sheet 
                                v-for="(syl, i) in feedback" 
                                :key="i" 
                                width="90" 
                                height="110" 
                                elevation="3"
                                rounded="lg" 
                                class="d-flex flex-column align-center justify-center cursor-pointer syllable-card"
                                :color="syl.status === 'excellent' ? 'amber-lighten-3' : 'grey-lighten-4'"
                                :class="syl.status === 'excellent' ? 'nacho-border' : ''"
                                @click="playSyllable(syl.text)"
                                v-ripple
                            >
                                <div class="text-h4 font-weight-black text-red-accent-2 mb-1">
                                    {{ syl.text }}
                                </div>

                                <v-chip size="x-small" variant="flat" class="font-weight-bold"
                                    :color="syl.status === 'excellent' ? 'green' : syl.status === 'good' ? 'orange' : 'red'">
                                    {{ Math.floor(syl.score) }}%
                                </v-chip>

                                <v-icon icon="mdi-volume-high" size="x-small" color="grey" class="mt-1 volume-hint" />
                            </v-sheet>
                        </div>

                        <div v-if="status === 'listening'" class="visualizer-bar"
                            :style="{ height: `${Math.min(micVolume * 2.5, 120)}px` }"></div>
                    </div>

                    <div class="pa-8 text-center bg-white">
                        <v-btn v-if="status === 'idle' || status === 'success' || status === 'error'" size="x-large"
                            color="blue-darken-2" rounded="pill" elevation="6" class="px-8" @click="startSession">
                            <v-icon start icon="mdi-microphone" />
                            Start Reading
                        </v-btn>

                        <div v-else-if="status === 'listening'" class="d-flex flex-column align-center">
                            <div class="text-h6 font-weight-bold text-blue mb-2 animate-pulse">
                                Listening...
                            </div>
                            <v-btn variant="text" color="grey" @click="stopSession">Cancel</v-btn>
                        </div>

                        <div v-else class="text-caption text-grey">
                            <v-progress-circular indeterminate color="blue" size="24" class="mr-2" />
                            Connecting...
                        </div>

                        <v-slide-y-transition>
                            <div v-if="errorMsg" class="mt-4 text-red font-weight-bold bg-red-lighten-5 pa-2 rounded">
                                {{ errorMsg }}
                            </div>
                        </v-slide-y-transition>

                    </div>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<style scoped>
.nacho-border {
    border: 3px solid #FFD54F !important;
}

.gap-3 {
    gap: 12px;
}

.visualizer-bar {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background: rgba(33, 150, 243, 0.15);
    border-top: 4px solid #2196F3;
    transition: height 0.05s ease;
    pointer-events: none;
    z-index: 0;
}

.animate-pulse {
    animation: pulse 1.5s infinite;
}

/* New CSS for Clickable Cards */
.cursor-pointer {
    cursor: pointer;
    transition: transform 0.1s;
}
.cursor-pointer:active {
    transform: scale(0.95);
}

.volume-hint {
    opacity: 0;
    transition: opacity 0.2s;
}
.syllable-card:hover .volume-hint {
    opacity: 1;
}

@keyframes pulse {
    0% { opacity: 1; }
    50% { opacity: 0.5; }
    100% { opacity: 1; }
}
</style>