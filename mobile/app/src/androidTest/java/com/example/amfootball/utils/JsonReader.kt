package com.example.amfootball.utils

import androidx.test.platform.app.InstrumentationRegistry

object JsonReader {

    fun readJsonFromAsserts(fileName: String): String {
        return try {
            val inputStream = InstrumentationRegistry.getInstrumentation().context.assets.open(fileName)
            inputStream.bufferedReader().use { it.readText() }
        } catch (e: Exception) {
            throw IllegalStateException("Não foi possível carregar o ficheiro JSON $fileName: ${e.message}")
        }
    }
}