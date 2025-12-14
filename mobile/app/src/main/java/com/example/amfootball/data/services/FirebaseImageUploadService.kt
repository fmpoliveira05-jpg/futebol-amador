package com.example.amfootball.data.services

import android.net.Uri
import com.example.amfootball.data.interfaces.services.ImageUploadService
import com.google.firebase.storage.FirebaseStorage
import kotlinx.coroutines.tasks.await
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Implementação concreta do serviço de upload de imagens utilizando o Firebase Storage.
 *
 * Esta classe utiliza a instância injetada do [FirebaseStorage] para realizar operações
 * de upload assíncronas e recuperar URLs públicos de download.
 *
 * @property storage Instância do Firebase Storage fornecida pelo módulo [FireBaseInstance].
 */
@Singleton
class FirebaseImageUploadService @Inject constructor(
    private val storage: FirebaseStorage
): ImageUploadService {

    /**
     * Realiza o upload de uma imagem para um caminho específico no Firebase Storage.
     *
     * Utiliza Coroutines para suspender a execução até que o upload esteja completo,
     * evitando o "callback hell" dos listeners tradicionais do Firebase.
     *
     * @param imageUri O URI local do ficheiro a ser enviado (geralmente obtido da galeria).
     * @param storagePath O caminho relativo onde o ficheiro será guardado no bucket (ex: "users/profile.jpg").
     * @return A URL pública de download (String) se o upload for bem-sucedido, ou `null` em caso de erro.
     */
    override suspend fun uploadImage(
        imageUri: Uri,
        storagePath: String
    ): String? {
        return try {
            val storageRef = storage.reference.child(storagePath)

            //Faz o upload e espera que termine (usando await da coroutine)
            storageRef.putFile(imageUri).await()

            //Pede o URL de download e espera que termine
            val downloadUrl = storageRef.downloadUrl.await()

            downloadUrl.toString()
        } catch (e: Exception) {
            e.printStackTrace()
            null // Retorna null em caso de erro
        }
    }


}