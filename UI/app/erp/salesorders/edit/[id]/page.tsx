
"use client"

import "react-datepicker/dist/react-datepicker.css";

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
import CustomerCombobox from '@/components/customer-combobox'
import DatePicker from "react-datepicker";


interface FormValues {
  username: string
  customer_id: number
}


function EditSalesOrderPage() {

      const {
      register,
      handleSubmit,
      formState: { errors },
    } = useForm<FormValues>();

  const onSubmit = handleSubmit((data) => {
    

  })

  const handleCustomerSelect = (value: any) => {
    console.log(value);
  }

  return (
    <form onSubmit={onSubmit}>
      <Grid
            templateColumns="repeat(3, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
          >
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.username}>
                <CustomerCombobox onChange={handleCustomerSelect} />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.username}>
                <Field.Label>Username</Field.Label>
                <DatePicker/>
                <Field.ErrorText>{errors.username?.message}</Field.ErrorText>
              </Field.Root>
            </Stack>
        </Grid>
      </form>
  )
}

export default EditSalesOrderPage;