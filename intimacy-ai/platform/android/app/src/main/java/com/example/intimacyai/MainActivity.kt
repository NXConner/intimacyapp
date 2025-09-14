package com.example.intimacyai
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.work.*
import okhttp3.*
import java.io.IOException
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey

class MainActivity: ComponentActivity(){
    private val client = OkHttpClient()

    override fun onCreate(savedInstanceState: Bundle?){
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                var status by remember { mutableStateOf("") }
                var apiBase by remember { mutableStateOf(load("ApiBaseUrl") ?: "http://10.0.2.2:5087") }
                var apiKey by remember { mutableStateOf(load("ApiKey") ?: "dev-key") }
                Column {
                    OutlinedTextField(apiBase, { apiBase = it }, label={ Text("API Base URL") })
                    OutlinedTextField(apiKey, { apiKey = it }, label={ Text("API Key") })
                    Row { Button(onClick={ save("ApiBaseUrl", apiBase); save("ApiKey", apiKey); status = "Saved" }){ Text("Save") } }
                    Button(onClick={ postAnalytics(apiBase, apiKey) { status = it } }){ Text("Post Analytics (stub)") }
                    Text(status)
                }
            }
        }
    }

    private fun postAnalytics(base: String, key: String, callback: (String)->Unit){
        val body = "{" +
                "\"anonymousUserId\":\"android\"," +
                "\"featureUsed\":\"stub\"," +
                "\"usageDurationSeconds\":1," +
                "\"platform\":\"android\"," +
                "\"appVersion\":\"0.0.1\"}"
        val req = Request.Builder()
            .url("$base/api/analytics")
            .addHeader("Content-Type","application/json")
            .addHeader("X-API-Key", key)
            .post(RequestBody.create(MediaType.parse("application/json"), body))
            .build()
        client.newCall(req).enqueue(object: Callback{
            override fun onFailure(call: Call, e: IOException) { callback("Failed: ${e.message}") }
            override fun onResponse(call: Call, response: Response) { callback("Status: ${response.code()}") }
        })
    }

    private fun prefs() = run {
        val key = MasterKey.Builder(this, MasterKey.DEFAULT_MASTER_KEY_ALIAS).setKeyScheme(MasterKey.KeyScheme.AES256_GCM).build()
        EncryptedSharedPreferences.create(this, "prefs", key, EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV, EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM)
    }
    private fun save(k:String, v:String){ prefs().edit().putString(k, v).apply() }
    private fun load(k:String): String? = prefs().getString(k, null)
}
