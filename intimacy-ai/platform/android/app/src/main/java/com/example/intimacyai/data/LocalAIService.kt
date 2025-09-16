package com.example.intimacyai.data

import android.graphics.Bitmap

interface LocalAIService {
    fun analyzeImage(bitmap: Bitmap): Map<String, Any>
}

class TensorflowLiteAIService: LocalAIService {
    override fun analyzeImage(bitmap: Bitmap): Map<String, Any> {
        // Stubbed local analysis. Replace with TFLite model inference.
        return mapOf(
            "arousal" to 0.4,
            "engagement" to 0.7,
            "quality" to 0.6
        )
    }
}

