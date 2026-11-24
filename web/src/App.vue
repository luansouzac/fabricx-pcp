<script setup>
import MainLayout from './layouts/MainLayout.vue'
import NotificacaoToast from './components/NotificacaoToast.vue'
</script>

<template>
  <v-app>
    <router-view v-if="$route.meta.publica" />
    <MainLayout v-else />
    <NotificacaoToast />
  </v-app>
</template>
