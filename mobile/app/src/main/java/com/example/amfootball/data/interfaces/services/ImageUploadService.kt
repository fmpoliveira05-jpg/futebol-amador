package com.example.amfootball.data.interfaces.services

import android.net.Uri

interface ImageUploadService {
    /**
     * Faz upload de uma imagem e retorna o URL de download.
     * @param imageUri O URI do ficheiro no telemóvel.
     * @param storagePath O caminho onde guardar no Firebase (ex: "users/profile.jpg" ou "teams/logo.jpg")
     * @return O URL público da imagem (String) ou null se falhar.
     */
    suspend fun uploadImage(imageUri: Uri, storagePath: String): String?
}