package com.example.amfootball.ui.viewModel

import com.google.firebase.Firebase
import com.google.firebase.auth.auth
import android.util.Log
import com.google.firebase.firestore.firestore


class ChatViewModel {
    fun fetchMyChatRooms() {
        val db = Firebase.firestore
        val myUserId = Firebase.auth.currentUser?.uid ?: return

        db.collection("chatRooms")
            .whereArrayContains("members", myUserId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                val rooms = querySnapshot.documents.map { doc ->
                    // Mapear o 'doc' para um data class 'ChatRoom'
                    // ex: doc.id, doc.getString("name"), doc.get("members")
                }
                // Atualizar o State do Compose com a lista de 'rooms'
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }
    }

    fun listenForMessages(roomId: String) {
        val db = Firebase.firestore

        val query = db.collection("chatRooms").document(roomId)
            .collection("messages")
            .orderBy("timestamp") // Ordenar pela data
            .limit(50) // Limitar às últimas 50

        // addSnapshotListener é o "Tempo Real".
        // Esta função será chamada AGORA com as mensagens
        // e DEPOIS, cada vez que uma nova mensagem for adicionada.
        query.addSnapshotListener { snapshot, error ->
            if (error != null) {
                Log.w("Chat", "Erro ao ouvir mensagens.", error)
                return@addSnapshotListener
            }

            if (snapshot != null) {
                val messages = snapshot.documents.map { doc ->
                    // Mapear 'doc' para um data class 'Message'
                    // ex: doc.getString("text"), doc.getString("senderId")
                }
                // Atualizar o State do Compose (ex: LazyColumn) com a lista 'messages'
            }
        }

        // (Lembre-se de guardar o 'ListenerRegistration' para o fechar
        // no onCleared() do ViewModel e evitar leaks de memória)
    }
}

