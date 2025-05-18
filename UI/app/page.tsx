"use client"

import React from 'react'
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  Button,
  GridItem,
  Image,
  Field,
} from '@chakra-ui/react'

import {
  PasswordInput,
} from "@/components/ui/password-input"



interface FormValues {
  username: string
  password: string
}

const App = () => {

    const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>();

  const router = useRouter();

  const onSubmit = handleSubmit((data) => {
    
    router.push('/erp')
  })

  return (
    <Grid
      templateColumns="repeat(1, 2fr)"
      gap={6}
      display="grid"
      width="25%"
      p="auto"
      m="auto"
    >
      <GridItem>
        <Image height="200px" width="200px" src="./kosmos_erp_med.png" p="auto" m="auto" />
      </GridItem>
      <form onSubmit={onSubmit}>
        <Stack gap="4" align="flex-start" maxW="md">
          <Field.Root invalid={!!errors.username}>
            <Field.Label>Username</Field.Label>
            <Input {...register("username")} maxLength={100} />
            <Field.ErrorText>{errors.username?.message}</Field.ErrorText>
          </Field.Root>

          <Field.Root invalid={!!errors.password}>
            <Field.Label>Password</Field.Label>
            <PasswordInput {...register("password")} maxLength={100}/>
            <Field.ErrorText>{errors.password?.message}</Field.ErrorText>
          </Field.Root>

          <Button type="submit" p="auto" m="auto">Submit</Button>
        </Stack>
      </form>
    </Grid>
  )
}


export default App
