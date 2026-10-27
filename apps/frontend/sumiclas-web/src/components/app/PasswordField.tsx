import { useState, type ComponentProps } from 'react';
import { PasswordToggle } from './PasswordToggle';
import { TextField } from './TextField';

type PasswordFieldProps = Omit<ComponentProps<typeof TextField>, 'type' | 'adornment'>;

export function PasswordField(props: PasswordFieldProps) {
  const [visible, setVisible] = useState(false);
  return (
    <TextField
      {...props}
      type={visible ? 'text' : 'password'}
      adornment={<PasswordToggle visible={visible} onToggle={() => setVisible((v) => !v)} />}
    />
  );
}
