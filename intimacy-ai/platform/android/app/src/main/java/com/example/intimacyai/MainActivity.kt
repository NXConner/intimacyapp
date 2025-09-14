package com.example.intimacyai
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.work.*
class MainActivity: ComponentActivity(){ override fun onCreate(savedInstanceState: Bundle?){ super.onCreate(savedInstanceState); setContent { MaterialTheme { Button(onClick={/* Windows Hello analog stub */}){ Text("Biometric (stub)") } } } } }
