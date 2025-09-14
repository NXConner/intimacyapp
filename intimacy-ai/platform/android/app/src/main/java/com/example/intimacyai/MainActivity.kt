package com.example.intimacyai
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.work.*
import okhttp3.*
import java.io.IOException

class MainActivity: ComponentActivity(){
    private val client = OkHttpClient()

    override fun onCreate(savedInstanceState: Bundle?){
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                var status by remember { mutableStateOf("") }
                Button(onClick={ postAnalytics { status = it } }){ Text("Post Analytics (stub)") }
                Text(status)
            }
        }
    }

    private fun postAnalytics(callback: (String)->Unit){
        val body = "{" +
                "\"anonymousUserId\":\"android\"," +
                "\"featureUsed\":\"stub\"," +
                "\"usageDurationSeconds\":1," +
                "\"platform\":\"android\"," +
                "\"appVersion\":\"0.0.1\"}"
        val req = Request.Builder()
            .url("http://10.0.2.2:5087/api/analytics")
            .addHeader("Content-Type","application/json")
            .addHeader("X-API-Key","dev-key")
            .post(RequestBody.create(MediaType.parse("application/json"), body))
            .build()
        client.newCall(req).enqueue(object: Callback{
            override fun onFailure(call: Call, e: IOException) { callback("Failed: ${e.message}") }
            override fun onResponse(call: Call, response: Response) { callback("Status: ${response.code()}") }
        })
    }
}
