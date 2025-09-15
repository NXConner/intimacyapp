package com.example.intimacyai
import android.content.Context
import androidx.work.Worker
import androidx.work.WorkerParameters
class AnalysisWorker(appContext: Context, params: WorkerParameters): Worker(appContext, params){ override fun doWork(): Result { return Result.success() } }
