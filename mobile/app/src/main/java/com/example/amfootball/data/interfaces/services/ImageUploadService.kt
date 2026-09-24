package com.example.amfootball.data.interfaces.services

import android.net.Uri

/**
 * Interface de serviço para lidar com o upload de ficheiros de imagem para um serviço de armazenamento
 * remoto, como o Firebase Storage.
 *
 * Esta interface abstrai a lógica de comunicação e gestão de ficheiros, permitindo que o
 * ViewModel ou o Repository usem este serviço sem se preocuparem com a implementação subjacente
 * (ex: se é Firebase, AWS S3, ou outro).
 */
interface ImageUploadService {
    /**
     * Faz upload de uma imagem e retorna o URL de download.
     * @param imageUri O URI do ficheiro no telemóvel.
     * @param storagePath O caminho onde guardar no Firebase (ex: "users/profile.jpg" ou "teams/logo.jpg")
     * @return O URL público da imagem (String) ou null se falhar.
     */
    suspend fun uploadImage(imageUri: Uri, storagePath: String): String?
}